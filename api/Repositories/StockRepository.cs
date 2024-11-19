using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _context;
        public StockRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Stock> CreateAsync(Stock stockModel)
        {
            await _context.Stocks.AddAsync(stockModel);
            await _context.SaveChangesAsync();
            return stockModel;
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
            if(stock == null){
                return null;
            }
            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            return stock;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
           var stocks = _context.Stocks.Include(c=> c.Comments).ThenInclude(a=> a.AppUser).AsQueryable();
            if(!string.IsNullOrWhiteSpace(query.CompanyName)){
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));

            }
            if(!string.IsNullOrWhiteSpace(query.Symbol)){
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));
            }

            if(!string.IsNullOrWhiteSpace(query.SortBy)){
               if(query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase)){
                    stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
               } 
            }

            var skipNumber = (query.PageNumber - 1) * query.PageSize;
            return await stocks.Skip(skipNumber).Take(query.PageSize).ToListAsync();
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
            return await _context.Stocks.Include(c=> c.Comments).ThenInclude(a => a.AppUser).FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Stock?> UpdateAsync(int id, UpdateStockDto updateDto)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);

            if(stock == null){
                return null;
            }   

            stock.Symbol = updateDto.Symbol;
            stock.Industry = updateDto.Industry;
            stock.Purchase = updateDto.Purchase;
            stock.MarketCap = updateDto.MarketCap;
            stock.CompanyName = updateDto.CompanyName;

            await _context.SaveChangesAsync(); 

            return stock; 
        }

        public Task<bool> StockExists (int id){
            return _context.Stocks.AnyAsync(s => s.Id == id);
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            return await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
        }
    }
}