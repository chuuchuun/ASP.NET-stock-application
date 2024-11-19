using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Data;
using api.Mappers;
using api.Dtos.Stock;
using Microsoft.EntityFrameworkCore;
using api.Repositories;
using api.Interfaces;
using api.Dtos.Comment;
using Microsoft.AspNetCore.Identity;
using api.Models;
using api.Extensions;
namespace api.Controllers
{
    
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(ICommentRepository commentRepo, 
        IStockRepository stockRepo, UserManager<AppUser> userManager){
            _commentRepo = commentRepo;
            _stockRepo = stockRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(){
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var comments = await _commentRepo.GetAllAsync();
            var commentsDto = comments.Select(x => x.ToCommentDto()).ToList();
            return Ok(commentsDto);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id){
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var comment = await _commentRepo.GetByIdAsync(id);
            if(comment == null){
                return NotFound();
            }
            var commentDto = comment.ToCommentDto();
            return Ok(commentDto);
        }

        [HttpPut]
        [Route("{id:int}")]

        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentDto commentDto){
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var comment = await _commentRepo.UpdateAsync(id, commentDto.ToCommentFromUpdate());
            if(comment == null){
                return NotFound();
            }
            return Ok(comment.ToCommentDto());
        }

        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromBody] CreateCommentRequestDto comment, [FromRoute] int stockId){
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            if(!await _stockRepo.StockExists(stockId)){
                return BadRequest("Stock does not exist");
            }
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            var commentModel = comment.ToCommentFromDto(stockId);
            commentModel.AppUserId = appUser.Id;
            await _commentRepo.CreateAsync(commentModel);

            return CreatedAtAction(nameof(GetById), new {id = commentModel.Id}, commentModel.ToCommentDto());
        }

        [HttpDelete]
        [Route("{id:int}")]

        public async Task<IActionResult> Delete([FromRoute] int id){
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var comment = await _commentRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}