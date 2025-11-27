using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Cadar_Raul_Lab4.PricePredictionModel;
using static Cadar_Raul_Lab4.DurationPredictionModel;

namespace Cadar_Raul_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        public IActionResult Price(PricePredictionModel.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel =
           mlContext.Model.Load(@"..\Cadar_Raul_Lab4\PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PricePredictionModel.ModelInput,
           PricePredictionModel.ModelOutput>(mlModel);
            // Try model on sample data to predict fair price
            PricePredictionModel.ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;
            return View(input);
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
    }
}
