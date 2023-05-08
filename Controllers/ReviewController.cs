using Doctor.BLL.Interface;
using Doctor.BLL.Services;
using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewServiceb)
        {
            _reviewService = reviewServiceb;
        }

        // GET: api/Review/id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = _reviewService.GetReviewById(id);
            return Ok(review);
        }

        // GET: api/Review
        [HttpGet("pagedReviews/{id}/")]
        public async Task<IActionResult> GetReviewsForDoctor([FromQuery] ReviewParams reviewParams, string id)
        {
            var reviews = await _reviewService.GetDoctorReviews(reviewParams, id);
            var responce = new PaginationHeader<ReviewDTO>(reviews, reviews.CurrentPage, reviews.PageSize, reviews.TotalCount);
            return Ok(responce);
        }

        // POST api/Review/create
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateReview([FromBody] Review model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var review = await _reviewService.Create(model);

            return Ok(review);
        }

        // PUT api/Review/Edit/id
        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditReviewById(int id, [FromBody] Review review)
        {
            var reviewToUpdate = await _reviewService.EditReviewById(id, review);  
            if (reviewToUpdate == null)
            {
                return NotFound($"Review with Id = {id} not found");
            }

            return Ok(reviewToUpdate);
        }


        // DELETE api/Review/Delete/id
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteReviewById(int id)
        {
            await _reviewService.DeleteReview(id);

            return Ok("Removed");

        }
    }
}
