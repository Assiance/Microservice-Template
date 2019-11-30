using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;
using System.Reflection;

namespace EfMicroservice.Common.Persistence.Extensions
{
    public static class MigrationHelperExtension
    {
        private static string _migrationFolder = "Migrations";
        public static void ComposeSqlUp(this MigrationBuilder builder, Type executingType)
        {
            var sqlString = ReadSqlFile(executingType, "Ups");
            builder.Sql(sqlString);
        }

        public static void ComposeSqlDown(this MigrationBuilder builder, Type executingType)
        {
            var sqlString = ReadSqlFile(executingType, "Downs");
            builder.Sql(sqlString);
        }

        private static string ReadSqlFile(Type executingType, string migrationType)
        {
            var myAttribute =
                (MigrationAttribute)Attribute.GetCustomAttribute(executingType, typeof(MigrationAttribute));

            var location = new Uri(Assembly.GetAssembly(executingType).GetName().CodeBase);
            var fileDirectoryInfo = new FileInfo(location.AbsolutePath).Directory;
            var path = $"{fileDirectoryInfo?.FullName}\\{_migrationFolder}\\{migrationType}\\{myAttribute.Id}.sql";
            var fileExists = File.Exists(path);
            if (!fileExists)
            {
                throw new Exception($"File Was not found with path: {path}");
            }
            var fileString = File.ReadAllText(path);
            return fileString;
        }
    }
}
