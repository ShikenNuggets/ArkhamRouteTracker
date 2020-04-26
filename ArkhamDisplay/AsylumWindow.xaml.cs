namespace ArkhamDisplay{
	public partial class AsylumWindow : BaseWindow{
		public AsylumWindow() : base(Game.Asylum){
			InitializeComponent();
			PostInitialize();
		}

		protected override void SetCurrentRoute(){
			currentRoute = "AsylumDefault";
		}
	}
}