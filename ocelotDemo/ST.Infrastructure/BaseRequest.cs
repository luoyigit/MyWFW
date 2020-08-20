using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
namespace ST.Infrastructure
{
    [MessagePackObject(true)]
    public class BaseRequest
    {
        public BaseRequest()
        {
        }

        public string ErrMsg { get; set; }

        public virtual bool IsValid()
        {
            var errList = new List<ValidationResult>();
            var result = Validator.TryValidateObject(this, new ValidationContext(this, null, null), errList);
            if (result)
                return true;
            else
            {
                ErrMsg = errList.First()?.ErrorMessage;
                return false;
            }
        }
    }
}
