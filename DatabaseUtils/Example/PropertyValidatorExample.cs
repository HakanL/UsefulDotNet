using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Haukcode.DatabaseUtils;
using Microsoft.EntityFrameworkCore;

namespace Haukcode.UsefulDotNet
{
    public class PropertyValidatorExample
    {
        public void DemoPropertyValidator(DbContext db)
        {
            var errors = PropertyValidator.ExecuteValidation(db);
            if (errors.Any())
                throw new DbUpdateException("Errors while validating the database models", innerException: null);
        }
    }
}
