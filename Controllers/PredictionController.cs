using Cadar_Raul_Lab4.Data;
using Cadar_Raul_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Cadar_Raul_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;
        
        public PredictionController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Price()
        {
            return View(new PricePredictionModel.ModelInput());
        }
        [HttpPost]

        public async Task<IActionResult> Price(PricePredictionModel.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"..\Cadar_Raul_Lab4\PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<
                PricePredictionModel.ModelInput,
                PricePredictionModel.ModelOutput>(mlModel);
            // Try model on sample data to predict fair price
            PricePredictionModel.ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;

            if (!string.IsNullOrEmpty(input.Payment_type))
            {

                var history = new PredictionHistory
                {
                    PassengerCount = input.Passenger_count,
                    TripTimeInSecs = input.Trip_time_in_secs,
                    TripDistance = input.Trip_distance,
                    PaymentType = input.Payment_type ?? "N/A",
                    PredictedPrice = result.Score,
                    CreatedAt = DateTime.Now
                };
                _context.PredictionHistories.Add(history);
                await _context.SaveChangesAsync();
                ViewBag.HistoryMessage = "Prediction saved to history.";
            }

            else
            {
                ViewBag.HistoryMessage = "Prediction not saved due to missing Payment Type.";
            }

            return View(input);
        }
        //public async Task<IActionResult> History()
        //{
        //    var history = await _context.PredictionHistories
        //    .OrderByDescending(p => p.CreatedAt)
        //    .ToListAsync();
        //    return View(history);
        //}

        [HttpGet]
        public async Task<IActionResult> History(
            string? paymentType,
            float? minPrice,
            float? maxPrice,
            string? sortOrder,
            DateTime? minDate,
            DateTime? maxDate)
        {
            var query = _context.PredictionHistories.AsQueryable();
            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }
            if (minDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date >= minDate.Value.Date);
            }
            if (maxDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date <= maxDate.Value.Date);
            }
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.PredictedPrice) //sortare default
            };
            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentMinDate = minDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentMinDate = maxDate?.ToString("yyyy-MM-dd");
            var result = await query.ToListAsync();
            return View(result);
        }

        public IActionResult Time(DurationPredictionModel.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel =
           mlContext.Model.Load(@"..\Cadar_Raul_Lab4\DurationPredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<DurationPredictionModel.ModelInput,
           DurationPredictionModel.ModelOutput>(mlModel);
            // Try model on sample data to predict fair time
            DurationPredictionModel.ModelOutput result = predEngine.Predict(input);
            ViewBag.Time = result.Score;
            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // 1. Numărul total de predicții
            var totalPredictions = await _context.PredictionHistories.CountAsync();
            // 2. Preț mediu per tip de plată + număr de predicții per tip
            var paymentTypeStats = await _context.PredictionHistories
            .GroupBy(p => p.PaymentType)
            .Select(g => new PaymentTypeStat
            {
                PaymentType = g.Key,
                AveragePrice = g.Average(x => x.PredictedPrice),
                Count = g.Count()
            })
            .ToListAsync();
            // 3. Distribuția prețurilor pe intervale (buckets)
            // Definim intervalele: 0-10, 10-20, 20-30, 30-50, >50 (exemplu)
            var allPredictions = await _context.PredictionHistories
            .Select(p => p.PredictedPrice)
            .ToListAsync();
            var buckets = new List<PriceBucketStat>
 {
 new PriceBucketStat { Label = "0 - 10" },
 new PriceBucketStat { Label = "10 - 20" },
 new PriceBucketStat { Label = "20 - 30" },
 new PriceBucketStat { Label = "30 - 50" },
 new PriceBucketStat { Label = "> 50" }
 };
            foreach (var price in allPredictions)
            {
                if (price < 10)
                    buckets[0].Count++;
                else if (price < 20)
                    buckets[1].Count++;
                else if (price < 30)
                    buckets[2].Count++;
                else if (price < 50)
                    buckets[3].Count++;
                else
                    buckets[4].Count++;
            }
            // 4. Construim ViewModel-ul
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets
            };
            return View(vm);
        }
    }
}
