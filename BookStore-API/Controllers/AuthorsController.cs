using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Get All Authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo($"{location}: Successfully got all Authors.");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{ e.Message} - {e.InnerException }");
            }
        }
        /// <summary>
        /// Get an Author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Authors record</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public  async   Task<IActionResult> GetAuthor(int id)
        {
            var location = GetControllerActionNames();
            try
            {
            _logger.LogInfo($"{location}: Attempted to get author with id: {id}");
            var author = await _authorRepository.FindById(id);
            if(author == null)
                {
                    _logger.LogWarn($"{location}: Author search got a null request for id: {id}");
                    return NotFound();
                }
            var response = _mapper.Map<AuthorDTO>(author);
            _logger.LogInfo($"{location}: Successfully got author with id: {id}");
            return Ok(response);

            }
            catch (Exception e)
            {
                return InternalError($"{ e.Message} - {e.InnerException }");
            }
        }


        

/// <summary>
        /// Create an author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public  async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Author submission attempted");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                // TODO check for uniqueness

                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Submitted data not valid");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSucess = await _authorRepository.Create(author);
                if (!isSucess)
                {
                    return InternalError("Author creation failed");
                }
                _logger.LogInfo($"{location}: Author Created");
                return Created("Author created ", new { author });

            }
            catch (Exception e)
            {
                return InternalError($"{ e.Message} - {e.InnerException }");
            }
        }


        /// <summary>
        /// Update author data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Author update attempted for id: {id}");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"{location}: Bad request was submitted, null or no id match");
                    return BadRequest();
                }

                var isExists = await _authorRepository.isExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Author with id: {id} not found");
                    return NotFound();
                }


                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Submitted data not valid");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSucess = await _authorRepository.Update(author);
                if (!isSucess)
                {
                    return InternalError($"{location}: Update operation failed");
                }
                _logger.LogInfo($"{location}: Author Created");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{ e.Message} - {e.InnerException }");
            }
        }



        /// <summary>
        /// Delete author data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                // Check if submitted request makes sense
                _logger.LogInfo($"{location}: Author delete attempted for id: {id}");
                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Bad request was submitted id less than 1");
                    return BadRequest();
                }
                // Create search variable and check if submitted request doesn't return null
                var isExists = await _authorRepository.isExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Author with id: {id} not found");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(id);
                var isSucess = await _authorRepository.Delete(author);
                if (!isSucess)
                {
                    return InternalError($"Delete operation failed");
                }
                _logger.LogInfo($"{location}: Author with id: {id} Deleted");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{ e.Message} - {e.InnerException }");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError (string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong ... again.");
        }
    }
}
