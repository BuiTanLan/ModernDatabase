using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Core.Entities.OrderNeo4j;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IGraphClient _client;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
         ITokenService tokenService, IMapper mapper, IGraphClient client)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
            _client = client;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetAddressUser()
        {
            var user = await _userManager.FindByUserByClaimsPrincipleWithAddressAsync(HttpContext.User);
            return _mapper.Map<Address, AddressDto>(user.Address);
        }

        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto address)
        {
            var user = await _userManager.FindByUserByClaimsPrincipleWithAddressAsync(HttpContext.User);
            var backUpUser = new AppUser();
            backUpUser = user;


            user.Address = _mapper.Map<AddressDto, Address>(address);
            var result = await _userManager.UpdateAsync(user);

            var newAddress = new UserNeo4j(user.Address);

            try
            {
                // update address
                var neo4jResult = await _client.Cypher.Match("(user:USER)")
                                              .Where<UserNeo4j>(e => e.BuyerEmail == user.Email)
                                              .Set("user = $newUSer")
                                              .Return(user => user.As<UserNeo4j>())
                                              .ResultsAsync;

                var temp = neo4jResult.Single();
                if (temp == null)
                {
                    await _userManager.UpdateAsync(backUpUser);
                    return BadRequest("Problem updating the user in neo4j");
                }
            }
            catch (Exception ex)
            {
                await _userManager.UpdateAsync(backUpUser);
                return BadRequest("Problem updating the user in neo4j");
            }

            if(result.Succeeded) return Ok(_mapper.Map<Address, AddressDto>(user.Address));
            return BadRequest("Problem updating the user");
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));
            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            try
            {
                await _client.ConnectAsync();
            }
            catch(Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.ToString()));
            }

            if (!_client.IsConnected)
                return BadRequest(new ApiResponse(400, "Not connected to neo4j"));

            if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse
                {
                    Errors = new[] {"Email address is in use"}
                });
            }
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            try
            {
                //add to graph
                var newUser = new UserNeo4j(user.Email);
                var addResult = await _client.Cypher.Create("(user:USER)")
                                                   .Set("user = $newUser")
                                                   .WithParams(new { newUser = newUser })
                                                   .Return(user => user.As<UserNeo4j>().BuyerEmail)
                                                   .ResultsAsync;
                var temp = addResult.Single();
                if (temp == null)
                {
                    await _userManager.DeleteAsync(user);
                    return BadRequest("Problem adding the user to neo4j");
                }
            }
            catch( Exception ex)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest("Problem adding the user to neo4j");
            }

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Token = await _tokenService.CreateToken(user),
                Email = user.Email
            };
        }
    }
}