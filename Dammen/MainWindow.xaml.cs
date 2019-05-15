using System.Windows;
using System.Windows.Controls;
using System;
using Dammen.Pages;
using FunctionalLayer;
using FunctionalLayer.Scenarios;
using Dammen.Windows;
using SacaDev.Diagnostics;
using Dammen.Pages.MultiPlayer;

namespace Dammen
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private IGame Game => App.container.Get<IGame>();
		private ILogger Logger => App.container.Get<ILogger>();
		//private IPage NavigateTo<IPage, TPage>(params object[] parameters) where TPage : Page, IPage
		//{
		//	var page = Activator.CreateInstance(); //App.container.Resolve<IPage, TPage>();
		//	this.bodyFrame.Navigate(page);
		//	return page;
		//}
		//private TPage NavigateTo<TPage>(params object[] parameters) where TPage : Page => NavigateTo<TPage, TPage>(parameters);
		private TPage NavigateTo<TPage>(TPage page) where TPage:Page {
			Logger.Write($"Navigating to the page {typeof(TPage).ToString()}");
			this.bodyFrame.Navigate(page);
			return page;
		}

		public MainWindow()
		{
			InitializeComponent();
			Logger.Write("Program started");
			Closed += MainWindow_Closed;
			GoToMainPage();
		}

		private void MainWindow_Closed(object sender, EventArgs e)
		{

			Logger.Write("Program stopped");
		}

		public void Quit() => Close();

		public void GoToSetupPage()
		{
			App.container.Remove<IGame>();
			var setupPage = new SetupGamePage();
			NavigateTo(setupPage);
			setupPage.Start += SetupPage_Start;
		}

		public void GoToMainPage() {
			var page = new MainMenuPage();

			NavigateTo(page);
			page.MainMenuItemSelected += MainPage_MainMenuItemSelected;
		}

		private void GoToMultiplayerMenu() {
			bool loggedIn = false;
			Page p;
			if(!loggedIn) {
				var page = new LoginPage();
				page.LoginSuccesful += Page_LoginSuccesful;
				p = page;
			}
			else {
				//go to game overview
				throw new NotImplementedException();
			}
			NavigateTo(p);
		}

		private void Page_LoginSuccesful(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void MainPage_MainMenuItemSelected(object sender, MainMenuItemSelectedEventHandler e)
		{
			Logger.Write($"Menuoption got selected on the main page with the following option: {e.SelectedOption}");
			switch(e.SelectedOption) {
				case MainMenuItemOption.SinglePlayer:
					GoToSetupPage();
					break;
				case MainMenuItemOption.Multiplayer:
					GoToMultiplayerMenu();
					break; 
				case MainMenuItemOption.ScenarioEditor:
					GoToScenarioEditor();
					break;
				case MainMenuItemOption.ScenarioOverview:
					GoToScenarioOverview();
					break;
				default:
					throw new Exception("Option not implemented");
			}
		}
		public void GoToScenarioOverview() {
			var page = new ScenarioSelectionPage();
			page.ScenarioChosen += Page_ScenarioChosen;
			NavigateTo(page);
		}

		private void Page_ScenarioChosen(object sender, ScenarioChosenEventArgs e)
		{
			App.container.Remove<IGame>();
			App.container.Singleton(e.Scenario);
			GoToGamePage();
		}

		public void GoToScenarioEditor() {
			App.container.Remove<IGame>();
			var setupScenarioPage = new SetupGamePage();
			setupScenarioPage.Start += SetupScenarioPage_Start;
			NavigateTo(setupScenarioPage);
		}

		private void GoToGamePage() {
			var gamepage = NavigateTo(new GamePage());
			//Setuppage has created the game instance, so it should be safe.

			this.Game.GameFinished += Game_GameFinished;
		}

		private void SetupScenarioPage_Start(object sender, GameEventArgs e)
		{
			App.container.Singleton(e.Game);
			ScenarioEditorPage scenarioEditorPage = new ScenarioEditorPage(App.container.Resolve<IGame>());
			NavigateTo(scenarioEditorPage);
		}

		private void SetupPage_Start(object sender, GameEventArgs e)
		{
			App.container.Singleton(e.Game);
			GoToGamePage();
		}

		private void Game_GameFinished(IPlayer winningPlayer)
		{
			var recapPage = new PostGameRecapPage(new PostGameRecapPageDataObject(
				this.Game,
				winningPlayer,
				this.Game.GetEnemyPlayer(winningPlayer),
				this.Game.Board
			));
			recapPage.Rematch += RecapPage_Rematch;
			recapPage.NewGame += RecapPage_NewGame;
			recapPage.ReturnToMainMenu += RecapPage_ReturnToMainMenu;
			this.bodyFrame.Navigate(recapPage);
		}

		private void RecapPage_ReturnToMainMenu(object sender, EventArgs e)
		{
			App.container.Remove<IGame>();
			GoToMainPage();
		}

		private void RecapPage_NewGame(object sender, EventArgs e)
		{
			App.container.Remove<IGame>();
			GoToSetupPage();
		}

		private void RecapPage_Rematch(object sender, EventArgs e)
		{
			this.Game.Rematch();
			NavigateTo(new GamePage());
		}

		private void MiReturnToMainMenu_Click(object sender, RoutedEventArgs e)
		{
			GoToMainPage();
		}

		private void MiSaveAsScenario_Click(object sender, RoutedEventArgs e)
		{
			if(Game == null)
				return;

			var scenarioNameDialog = new ScenarioCreateDialog();
			scenarioNameDialog.Owner = Application.Current.MainWindow;
			scenarioNameDialog.ShowDialog();

			var result = scenarioNameDialog.DialogResult;
			var manager = new ScenarioManager(Constants.SCENARIOSTOREPATH);
			if(result.HasValue && result.Value) {
				var name = scenarioNameDialog.ScenarioName;
				manager.SaveScenario(name, Game);
			}
		}
	}
}