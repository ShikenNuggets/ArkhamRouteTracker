namespace ArkhamDisplay{
	public partial class AsylumWindow : BaseWindow{
		public AsylumWindow() : base(Game.Asylum){
			InitializeComponent();
			PostInitialize();
		}

		protected override void SetCurrentRoute(){
			currentRoute = "AsylumDefault";
		}

		protected override string UpdateRiddleCount(){
			return saveParser.GetMatch(@"\b\d*\/240\b");
		}
	}
}