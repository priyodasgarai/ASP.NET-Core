using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreApiResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using eComApp.Models;
using eComApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using eComApp.Extensions;

namespace eComApp.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepository _stockRepo;
        private readonly IPortfolioRepository _portfolioRepo;
        private readonly ILogger<PortfolioController> _logger;
        public PortfolioController(
        UserManager<AppUser> userManager,
        IStockRepository stockRepo,
        IPortfolioRepository portfolioRepo,
        ILogger<PortfolioController> logger
        )
        {
            _stockRepo = stockRepo;
            _userManager = userManager;
            _portfolioRepo = portfolioRepo;
            _logger = logger;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio()
        {
            try
            {
                var username = User.GetUsername();
                var appUser = await _userManager.FindByNameAsync(username);
                var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
                return CustomResult("Portfolio ", userPortfolio, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio(string symbol)
        {
            try
            {
                var username = User.GetUsername();
                var appUser = await _userManager.FindByNameAsync(username);
                var stock = await _stockRepo.GetBySymbolAsync(symbol);
                if (stock == null) return BadRequest("Stock not found");
                var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
                if (userPortfolio.Any(e => e.Symbol.ToLower() == symbol.ToLower())) return BadRequest("Cannot add same stock to portfolio");
                var portfolioModel = new Portfolio
                {
                    StockId = stock.Id,
                    AppUserId = appUser.Id
                };
                await _portfolioRepo.CreateAsync(portfolioModel);
                if (portfolioModel == null)
                {
                    return CustomResult("Could not create ", userPortfolio, HttpStatusCode.BadRequest);
                }
                else
                {
                    return CustomResult("Portfolio created ", Created(), HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol)
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);
            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
            var filteredStock = userPortfolio.Where(s => s.Symbol.ToLower() == symbol.ToLower()).ToList();
            if (filteredStock.Count() == 1)
            {
                await _portfolioRepo.DeletePortfolio(appUser, symbol);

            }
            else
            {
                return CustomResult("Stock not in your portfolio ", HttpStatusCode.BadRequest);
            }
            return Ok();
        }

    }
}