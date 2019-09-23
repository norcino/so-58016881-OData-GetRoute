using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class Value
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Authorize]
    public class ValuesController : ODataController
    {
        private List<Value> _values;

        public ValuesController()
        {
            _values = new List<Value>
            {
                new Value {Id = 1, Name = "A1"},
                new Value {Id = 2, Name = "A2"},
                new Value {Id = 3, Name = "A3"},
                new Value {Id = 4, Name = "A4"},
                new Value {Id = 5, Name = "A5"},
                new Value {Id = 6, Name = "A6"},
                new Value {Id = 7, Name = "A7"},
                new Value {Id = 11, Name = "B1"},
                new Value {Id = 12, Name = "B2"},
                new Value {Id = 13, Name = "B3"},
                new Value {Id = 14, Name = "B4"},
                new Value {Id = 15, Name = "B5"},
                new Value {Id = 16, Name = "B6"},
                new Value {Id = 17, Name = "B7"}
            };
        }

        // GET {tenant}/odata/values
        [EnableQuery]
        public IQueryable<Value> Get()
        {
            return _values.AsQueryable();
        }

        // GET {tenant}/odata/values/5
        [EnableQuery]
        public ActionResult<Value> Get([FromODataUri] int key)
        {
            if(_values.Any(v => v.Id == key))
                return _values.Single(v => v.Id == key);
        
            return NotFound();
        }
    }
}
