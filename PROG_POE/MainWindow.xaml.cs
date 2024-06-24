using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PROG_POE
{
    public partial class MainWindow : Window
    {
        private List<Recipe> recipes = new List<Recipe>();
        private Recipe selectedRecipe = null;

        public MainWindow()
        {
            InitializeComponent();
            listBoxRecipes.SelectionChanged += ListBoxRecipes_SelectionChanged;
            AddRecipe.Click += AddRecipe_Click;
            buttonAddIngredient.Click += ButtonAddIngredient_Click;
            buttonAddStep.Click += ButtonAddStep_Click;
            buttonFilter.Click += ButtonFilter_Click;
            buttonDeleteRecipe.Click += ButtonDeleteRecipe_Click;
            listBoxSteps.MouseDoubleClick += ListBoxSteps_MouseDoubleClick;

        }

        //Method that adds the entered recipe name to the listbox after the add recipe button is clicked
        private void AddRecipe_Click(object sender, RoutedEventArgs e)
        {
            var recipeName = textBoxRecipeName.Text;
            if (!string.IsNullOrWhiteSpace(recipeName))
            {
                var recipe = new Recipe(recipeName);
                recipes.Add(recipe);
                UpdateRecipeListBox();
                textBoxRecipeName.Clear();
            }
        }

        //Method that updates the list box
        private void UpdateRecipeListBox()
        {
            listBoxRecipes.Items.Clear();
            foreach (var recipe in recipes.OrderBy(r => r.Name))
            {
                listBoxRecipes.Items.Add(recipe.Name);
            }
        }

        //Displays the recipe information when a recipe is clicked in the list box
        private void ListBoxRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxRecipes.SelectedItem != null)
            {
                var recipeName = listBoxRecipes.SelectedItem.ToString();
                selectedRecipe = recipes.FirstOrDefault(r => r.Name == recipeName);
                DisplayIngredients();
                DisplaySteps();
                UpdateTotalCaloriesLabel();
                textBoxCalories.Clear();
                textBoxIngredientName.Clear();
                comboBoxFoodGroup.SelectedIndex = -1;
                textBoxQuantity.Clear();
                if (selectedRecipe != null)
                {
                    selectedRecipe.OnCaloriesChanged += SelectedRecipe_OnCaloriesChanged;
                }
            }
        }

        //Updates the total calories label
        private void SelectedRecipe_OnCaloriesChanged(object sender, EventArgs e)
        {
            UpdateTotalCaloriesLabel();
        }

        //Displays the ingredients in the data grid view
        private void DisplayIngredients()
        {
            if (selectedRecipe != null)
            {
                dataGridViewIngredients.ItemsSource = null;
                dataGridViewIngredients.ItemsSource = selectedRecipe.Ingredients;
            }
        }

        //Displays the recipe steps in the list box
        private void DisplaySteps()
        {
            if (selectedRecipe != null)
            {
                listBoxSteps.ItemsSource = null;
                var numberedSteps = selectedRecipe.IncompleteSteps.Select((step, index) => $"{index + 1}. {step}").ToList();
                listBoxSteps.ItemsSource = numberedSteps;
            }
        }

        //Receives input regarding Ingredients
        private void ButtonAddIngredient_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipe != null)
            {
                try
                {
                    var ingredientName = textBoxIngredientName.Text;
                    if (string.IsNullOrWhiteSpace(ingredientName))
                    {
                        MessageBox.Show("Ingredient name cannot be empty.");
                        return;
                    }

                    var quantity = textBoxQuantity.Text;
                    if (string.IsNullOrWhiteSpace(quantity))
                    {
                        MessageBox.Show("Please enter a valid quantity.");
                        return;
                    }

                    if (!double.TryParse(textBoxCalories.Text, out double calories) || calories < 0)
                    {
                        MessageBox.Show("Please enter valid calories.");
                        return;
                    }

                    var foodGroup = (comboBoxFoodGroup.SelectedItem as ComboBoxItem)?.Content.ToString();
                    if (string.IsNullOrWhiteSpace(foodGroup))
                    {
                        MessageBox.Show("Please select a food group.");
                        return;
                    }

                    selectedRecipe.AddIngredient(ingredientName, quantity, calories, foodGroup);
                    DisplayIngredients();
                    textBoxCalories.Clear();
                    textBoxIngredientName.Clear();
                    comboBoxFoodGroup.SelectedIndex = -1;
                    textBoxQuantity.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a recipe first.");
            }
        }

        //Adds the entered step to the recipe
        private void ButtonAddStep_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipe != null)
            {
                var step = textBoxStep.Text;
                if (!string.IsNullOrWhiteSpace(step))
                {
                    selectedRecipe.AddStep(step);
                    DisplaySteps();
                    textBoxStep.Clear();
                }
            }
        }

        //Removes the completed step from the list box when the rstep is double clicked
        private void ListBoxSteps_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listBoxSteps.SelectedItem != null)
            {
                var selectedStep = listBoxSteps.SelectedItem.ToString();
                var stepText = selectedStep.Substring(selectedStep.IndexOf(' ') + 1);

                selectedRecipe.CompleteStep(stepText);
                DisplaySteps();
            }
        }
        
        //Filters the recipe names
        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
        {
            var filteredRecipes = new List<Recipe>();

            if (radioButtonIngredient.IsChecked == true)
            {
                var ingredient = textBoxIngredientFilter.Text;
                filteredRecipes = recipes.Where(r => r.Ingredients.Any(i => i.Name.IndexOf(ingredient, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }
            else if (radioButtonFoodGroup.IsChecked == true)
            {
                var foodGroup = textBoxFoodGroupFilter.Text;
                filteredRecipes = recipes.Where(r => r.Ingredients.Any(i => i.FoodGroup.IndexOf(foodGroup, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }
            else if (radioButtonCalories.IsChecked == true)
            {
                if (double.TryParse(textBoxCaloriesFilter.Text, out double maxCalories))
                {
                    filteredRecipes = recipes.Where(r => r.TotalCalories <= maxCalories).ToList();
                }
            }

            listBoxRecipes.Items.Clear();
            foreach (var recipe in filteredRecipes.OrderBy(r => r.Name))
            {
                listBoxRecipes.Items.Add(recipe.Name);
            }
        }

        //Deletes the recipe
        private void ButtonDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipe != null)
            {
                recipes.Remove(selectedRecipe);
                UpdateRecipeListBox();
                selectedRecipe = null;
                dataGridViewIngredients.ItemsSource = null;
                listBoxSteps.ItemsSource = null;
                labelTotalCalories.Content = "Total Calories: 0";
            }
        }

        //Updates the total calories label
        private void UpdateTotalCaloriesLabel()
        {
            if (selectedRecipe != null)
            {
                labelTotalCalories.Content = $"Total Calories: {selectedRecipe.TotalCalories}";
            }
        }

        //Changes the visibility of the textboxes when a radio button is seleted.
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            textBoxIngredientFilter.Visibility = radioButtonIngredient.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            textBoxFoodGroupFilter.Visibility = radioButtonFoodGroup.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            textBoxCaloriesFilter.Visibility = radioButtonCalories.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        //Shows all recipes
        private void buttonAllRecipes_Click(object sender, RoutedEventArgs e)
        {
            UpdateRecipeListBox();
        }
    }
}
