public class PooledPlortLockParticle : PooledConfigurableSceneParticle
{
	public MinMaxGradientData coreStartColor;

	public MinMaxGradientData wispStartColor;

	public MinMaxGradientData burstStartColor;

	protected override void ConfigureParticles()
	{
		SetColors(null, coreStartColor);
		SetColors("Wisps", wispStartColor);
		SetColors("FX Bursts", burstStartColor);
	}
}
