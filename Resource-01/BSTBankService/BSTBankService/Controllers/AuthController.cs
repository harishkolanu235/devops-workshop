using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BSTBankService.Data;
using BSTBankService.Models;
using BSTBankService.Models.AppModels;
using BSTBankService.Models.ViewModels;
//using BSTBankService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BSTBankService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        //private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;

        public AuthController(UserManager<AppUser> userManager, IMapper mapper, 
            ApplicationDbContext appDbContext,
            //IJwtFactory jwtFactory, 
            IOptions<JwtIssuerOptions> jwtOptions)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
            //_jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdentity = _mapper.Map(model, typeof(RegistrationViewModel),typeof(AppUser)) as AppUser;

            var result = await _userManager.CreateAsync(userIdentity, model.Password);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to register the user");
                return new BadRequestObjectResult(ModelState);
            }

            Customer customer = new Customer { 
                IdentityId = userIdentity.Id,                
            };

            await _appDbContext.Customers.AddAsync(customer);
            await _appDbContext.SaveChangesAsync();

            return new OkObjectResult(new { status="Created", user= customer });
        }

        
    }
}
