using Microsoft.AspNetCore.Mvc;
using Khairah_.Models;
using Khairah_.Data;

namespace temple2.Views.Shared.Components
{
    [ViewComponent(Name = "NavDropDown")]

    public class NavDropDownComponents : ViewComponent
    {
        private readonly NgoProjectdbContext db;

        public NavDropDownComponents(NgoProjectdbContext db)
        {

            this.db = db;
        }
        public IViewComponentResult Invoke()
        {
            var causetype = db.CauseTypes.ToList();
            return View(causetype);
            
        }

    }
}
