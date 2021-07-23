using System;
using System.Threading.Tasks;
using API.Dtos;
using API.Extensions;
using AutoMapper;
using Core.Entities;
using Core.Entities.CommentCassandra;
using Core.Entities.Identity;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class  CommentController : BaseApiController
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(ICommentService commentService, UserManager<AppUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpGet("{productID}")]
        public async Task<IActionResult> GetCommentByProduct(string productID)
        {
            try
            {
                return Ok(await _commentService.GetAllCommentsByProduct(productID));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentRequestDto commentDto)
        {
            var user = await _userManager.FindByUserByClaimsPrincipleWithAddressAsync(HttpContext.User);
            var comment = new Comment
            {
                Email = user.Email,
                UserName = user.DisplayName,
                Content = commentDto.Content,
                ProductId = commentDto.ProductId
            };
            return Ok(await _commentService.AddComment(comment));

        }
    }

}