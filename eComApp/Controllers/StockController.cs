using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreApiResponse;
using eComApp.Data;
using eComApp.Dtos.Stock;
using eComApp.Helpers;
using eComApp.Interfaces;
using eComApp.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace eComApp.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : BaseController
    {

        private readonly ILogger<StockController> _logger;
        private readonly IStockRepository _stockRepo;
        public StockController(IStockRepository stockRepo, ILogger<StockController> logger)
        {
            _logger = logger;
            _stockRepo = stockRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var stocks = await _stockRepo.GetAllAsync(query);
                var stockDto = stocks.Select(s => s.ToStockDto()).ToList();
                if (stockDto == null || stockDto.Count() == 0)
                {
                    return CustomResult("Data not found", HttpStatusCode.NotFound);
                }
                return CustomResult("Data loaded successfully", stockDto, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }

        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var stock = await _stockRepo.GetByIdAsync(id);
                if (stock == null)
                {
                    return CustomResult("Data not found", HttpStatusCode.NotFound);
                }

                return CustomResult("Data loaded successfully", stock.ToStockDto(), HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                // _logger.LogInformation(ex.Message);
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var stockModel = stockDto.ToStockFromCreateDTO();
                await _stockRepo.CreateAsync(stockModel);
                var result = CreatedAtAction(nameof(GetById), new { id = stockModel.Id }, stockModel.ToStockDto());
                return CustomResult("Data added successfully", result.Value, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                // _logger.LogInformation(ex.Message);
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var stockModel = await _stockRepo.UpdateAsync(id, updateDto);
                if (stockModel == null)
                {
                    return CustomResult("Data not found", HttpStatusCode.NotFound);
                }


                return CustomResult("Data Updated successfully", stockModel.ToStockDto(), HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                // _logger.LogInformation(ex.Message);
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var stockModel = await _stockRepo.DeleteAsync(id);
                if (stockModel == null)
                {
                    return CustomResult("Data not found", HttpStatusCode.NotFound);
                }

                return CustomResult("Data delete successfully", HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                // _logger.LogInformation(ex.Message);
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}