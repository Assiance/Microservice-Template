using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EfMicroservice.Core.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyVersionInfoConfiguration(this ModelBuilder modelBuilder)
        {;
            var converter = new ValueConverter<byte[], long>(
                v => BitConverter.ToInt64(v, 0),
                v => BitConverter.GetBytes(v));

            var versionInfoTypes = GetTypesAssignableFrom(typeof(IVersionInfo));

            foreach (var versionInfoType in versionInfoTypes)
            {
                modelBuilder.Entity(versionInfoType)
                    .Property("RowVersion")
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .HasConversion(converter)
                    .IsRowVersion();
            }
        }

        private static IEnumerable<Type> GetTypesAssignableFrom(Type assignableType)
        {
            return GetInitialAssembly().DefinedTypes
                .Where(type => type.ImplementedInterfaces.Any(i => i == assignableType));
        }

        private static Assembly GetInitialAssembly()
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            var initialAssembly = frames.Select(frame => frame?.GetMethod()?.ReflectedType?.Assembly)
                .Distinct()
                .Skip(1)
                .First();

            return initialAssembly;
        }
    }
}
