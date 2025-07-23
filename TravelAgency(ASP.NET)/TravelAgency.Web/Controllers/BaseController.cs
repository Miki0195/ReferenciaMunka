using System.Collections.Generic;
using AutoMapper;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ELTE.TravelAgency.Web.Controllers
{
	/// <summary>
	/// Vezrlő ősosztálya.
	/// </summary>
	public class BaseController : Controller
    {
        // a logikát szolgáltatás osztály mögé rejtjük
        protected readonly ICityService _cityService;

        // entitás -> dto mapper
        protected readonly IMapper _mapper;

        public BaseController(ICityService cityService, IMapper mapper)
        {
            _cityService = cityService;
            _mapper = mapper;
        }

        /// <summary>
		/// Egy akció meghívása után végrehajtandó metódus.
		/// </summary>
		/// <param name="context">Az akció kontextus argumentuma.</param>
		public override void OnActionExecuted(ActionExecutedContext context)
	    {
		    base.OnActionExecuted(context);

            // a minden oldalról elérhető információkat össze gyűjtjük
            var cities = _cityService.GetAllAsync().Result;
            var cityViewModels = _mapper.Map<List<CityViewModel>>(cities);
            ViewBag.Cities = cityViewModels;
        }
	}
}