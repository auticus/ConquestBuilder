using System;
using System.Collections.Generic;
using System.Text;
using ConquestController.Models.Input;

namespace ConquestController.Analysis.Components
{
    public class Movement : BaseComponent
    {
        public static double CalculateOutput<T>(ConquestInput<T> model)
        {
            var movementScore = (double)model.Move;
            if (model.IsFluid == 1 && model.IsFly == 0) movementScore *= IsFluidWeight;
            if (model.IsFluid == 0 && model.IsFly == 1) movementScore *= IsFlyWeight;
            if (model.IsFluid == 1 && model.IsFly == 1) movementScore *= IsFluidFlyWeight;

            return movementScore;
        }
    }
}
