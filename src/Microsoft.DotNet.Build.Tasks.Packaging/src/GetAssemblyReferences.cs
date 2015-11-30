﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace CoreFx.Build.Tasks
{
    public class GetAssemblyReferences : ITask
    {
        public IBuildEngine BuildEngine
        {
            get;
            set;
        }

        public ITaskHost HostObject
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Assemblies
        {
            get;
            set;
        }


        [Output]
        public ITaskItem[] ReferencedAssemblies
        {
            get;
            set;
        }

        public bool Execute()
        {
            if (Assemblies == null || Assemblies.Length == 0)
                return true;

            List<ITaskItem> references = new List<ITaskItem>();

            foreach (var assemblyItem in Assemblies)
            {
                try
                {
                    using (PEReader peReader = new PEReader(new FileStream(assemblyItem.ItemSpec, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.Read)))
                    {
                        MetadataReader reader = peReader.GetMetadataReader();
                        foreach (var handle in reader.AssemblyReferences)
                        {
                            AssemblyReference reference = reader.GetAssemblyReference(handle);
                            TaskItem referenceItem = new TaskItem(reader.GetString(reference.Name));
                            assemblyItem.CopyMetadataTo(referenceItem);
                            referenceItem.SetMetadata("Version", reference.Version.ToString());
                            references.Add(referenceItem);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Ignore invalid assemblies
                }
            }

            ReferencedAssemblies = references.ToArray();

            return true;
        }
    }
}
