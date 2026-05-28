public class PooledPlortSwitchParticle : PooledConfigurableSceneParticle
{
	public MinMaxGradientData coreStartColor;

	public MinMaxGradientData sparkleStartColor;

	public MinMaxGradientData wispStartColor;

	protected override void ConfigureParticles()
	{
		SetColors(null, coreStartColor);
		SetColors("FX Sparkle", sparkleStartColor);
		SetColors("Wisps", wispStartColor);
	}
}
