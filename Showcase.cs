using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Haukcode.UsefulDotNet
{
    public class Showcase
    {
        public void DemoPropertyValidator(DbContext db)
        {
            var errors = PropertyValidator.ExecuteValidation(db);
            if (errors.Any())
                throw new DbUpdateException("Errors while validating the database models", innerException: null);
        }
    }
}
