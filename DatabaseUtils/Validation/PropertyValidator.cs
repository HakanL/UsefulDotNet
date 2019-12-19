using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Haukcode.DatabaseUtils
{
    public static class PropertyValidator
    {
        public static IList<ValidationResult> ExecuteValidation(DbContext context)
        {
            var errors = new List<ValidationResult>();

            foreach (var entry in
                context.ChangeTracker.Entries()
                    .Where(e =>
                        (e.State == EntityState.Added) ||
                        (e.State == EntityState.Modified)))
            {
                object entity = entry.Entity;
                var valContext = new ValidationContext(entity);
                var entityErrors = new List<ValidationResult>();
                if (!Validator.TryValidateObject(
                    entity, valContext, entityErrors, validateAllProperties: true))
                {
                    // Hack
                    entityErrors = entityErrors.Where(x => !x.MemberNames.Any(m => m == "RowVersion")).ToList();
                    errors.AddRange(entityErrors);
                }

                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.ClrType == typeof(string))
                    {
                        var annotation = prop.Metadata.FindAnnotation("MaxLength");

                        if (annotation != null && annotation.Value is int)
                        {
                            int currentLen = ((string)prop.CurrentValue)?.Length ?? 0;
                            if (currentLen > (int)annotation.Value)
                            {
                                // String too long
                                errors.Add(new ValidationResult(
                                    $"String overflow, length = {currentLen}, max = {annotation.Value}",
                                    new string[] { prop.Metadata.Name }));
                            }
                        }
                    }
                }
            }

            return errors;
        }
    }
}
