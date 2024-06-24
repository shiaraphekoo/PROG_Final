using System;
using System.Collections.Generic;
using System.Linq;

namespace PROG_POE
{
    public class Recipe
    {
        //Recipe attributes and lists
        public string Name { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public List<string> Steps { get; set; }
        public List<string> IncompleteSteps { get; set; }

        //Method to add input to lists
        public Recipe(string name)
        {
            Name = name;
            Ingredients = new List<Ingredient>();
            Steps = new List<string>();
            IncompleteSteps = new List<string>(); 
        }

        //Calculates the total calories for a recipe
        public double TotalCalories => Ingredients.Sum(i => i.Calories);

        public void AddIngredient(string name, string quantity, double calories, string foodGroup)
        {
            Ingredients.Add(new Ingredient { Name = name, Quantity = quantity, Calories = calories, FoodGroup = foodGroup });
            CheckCalories();
            OnCaloriesChanged?.Invoke(this, EventArgs.Empty);
        }
        
        //Method to add steps
        public void AddStep(string step)
        {
            Steps.Add(step);
            IncompleteSteps.Add(step);
        }

        //Method to check if calories are over 300 and send an alert to the user
        private void CheckCalories()
        {
            if (TotalCalories > 300)
            {
                OnCaloriesExceeded?.Invoke(this, EventArgs.Empty);
            }
        }

        //Method that removes completed steps
        public void CompleteStep(string step)
        {
            IncompleteSteps.Remove(step);
        }

        public event EventHandler OnCaloriesExceeded;
        public event EventHandler OnCaloriesChanged;
    }
}
