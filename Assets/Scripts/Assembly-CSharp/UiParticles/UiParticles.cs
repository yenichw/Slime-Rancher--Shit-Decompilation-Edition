using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UiParticles
{
	[RequireComponent(typeof(ParticleSystem))]
	public class UiParticles : MaskableGraphic
	{
		[FormerlySerializedAs("m_ParticleSystem")]
		private ParticleSystem m_ParticleSystem;

		[FormerlySerializedAs("m_ParticleSystemRenderer")]
		private ParticleSystemRenderer m_ParticleSystemRenderer;

		private ParticleSystem.Particle[] m_Particles;

		public ParticleSystem ParticleSystem
		{
			get
			{
				return m_ParticleSystem;
			}
			set
			{
				if (SetPropertyUtility.SetClass(ref m_ParticleSystem, value))
				{
					SetAllDirty();
				}
			}
		}

		public ParticleSystemRenderer particleSystemRenderer
		{
			get
			{
				return m_ParticleSystemRenderer;
			}
			set
			{
				if (SetPropertyUtility.SetClass(ref m_ParticleSystemRenderer, value))
				{
					SetAllDirty();
				}
			}
		}

		public override Texture mainTexture
		{
			get
			{
				if (material != null && material.mainTexture != null)
				{
					return material.mainTexture;
				}
				return Graphic.s_WhiteTexture;
			}
		}

		protected override void Awake()
		{
			ParticleSystem component = GetComponent<ParticleSystem>();
			ParticleSystemRenderer component2 = GetComponent<ParticleSystemRenderer>();
			if (m_Material == null)
			{
				m_Material = component2.sharedMaterial;
			}
			base.Awake();
			ParticleSystem = component;
			particleSystemRenderer = component2;
		}

		public override void SetMaterialDirty()
		{
			base.SetMaterialDirty();
			if (particleSystemRenderer != null)
			{
				particleSystemRenderer.sharedMaterial = m_Material;
			}
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (ParticleSystem == null)
			{
				base.OnPopulateMesh(toFill);
			}
			else
			{
				GenerateParticlesBillboards(toFill);
			}
		}

		private void InitParticlesBuffer()
		{
			ParticleSystem.MainModule main = ParticleSystem.main;
			if (m_Particles == null || m_Particles.Length < main.maxParticles)
			{
				m_Particles = new ParticleSystem.Particle[main.maxParticles];
			}
		}

		private void GenerateParticlesBillboards(VertexHelper vh)
		{
			InitParticlesBuffer();
			int particles = ParticleSystem.GetParticles(m_Particles);
			vh.Clear();
			for (int i = 0; i < particles; i++)
			{
				DrawParticleBillboard(m_Particles[i], vh);
			}
		}

		private void DrawParticleBillboard(ParticleSystem.Particle particle, VertexHelper vh)
		{
			Vector3 vector = particle.position;
			Quaternion quaternion = Quaternion.Euler(particle.rotation3D);
			if (ParticleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World)
			{
				vector = base.rectTransform.InverseTransformPoint(vector);
			}
			Vector3 currentSize3D = particle.GetCurrentSize3D(ParticleSystem);
			Vector3 vector2 = new Vector3((0f - currentSize3D.x) * 0.5f, currentSize3D.y * 0.5f);
			Vector3 vector3 = new Vector3(currentSize3D.x * 0.5f, currentSize3D.y * 0.5f);
			Vector3 vector4 = new Vector3(currentSize3D.x * 0.5f, (0f - currentSize3D.y) * 0.5f);
			Vector3 vector5 = new Vector3((0f - currentSize3D.x) * 0.5f, (0f - currentSize3D.y) * 0.5f);
			vector2 = quaternion * vector2 + vector;
			vector3 = quaternion * vector3 + vector;
			vector4 = quaternion * vector4 + vector;
			vector5 = quaternion * vector5 + vector;
			Color32 currentColor = particle.GetCurrentColor(ParticleSystem);
			int currentVertCount = vh.currentVertCount;
			Vector2[] array = new Vector2[4];
			if (!ParticleSystem.textureSheetAnimation.enabled)
			{
				array[0] = new Vector2(0f, 0f);
				array[1] = new Vector2(0f, 1f);
				array[2] = new Vector2(1f, 1f);
				array[3] = new Vector2(1f, 0f);
			}
			else
			{
				ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = ParticleSystem.textureSheetAnimation;
				float num = particle.startLifetime - particle.remainingLifetime;
				float num2 = particle.startLifetime / (float)textureSheetAnimation.cycleCount;
				float time = num % num2 / num2;
				float num3 = textureSheetAnimation.frameOverTime.Evaluate(time);
				int num4 = textureSheetAnimation.numTilesY * textureSheetAnimation.numTilesX;
				float num5 = Mathf.Clamp(Mathf.Floor(num3 * (float)num4), 0f, num4 - 1);
				int num6 = (int)num5 % textureSheetAnimation.numTilesX;
				int num7 = (int)num5 / textureSheetAnimation.numTilesY;
				float num8 = 1f / (float)textureSheetAnimation.numTilesX;
				float num9 = 1f / (float)textureSheetAnimation.numTilesY;
				num7 = textureSheetAnimation.numTilesY - 1 - num7;
				float num10 = (float)num6 * num8;
				float num11 = (float)num7 * num9;
				float x = num10 + num8;
				float y = num11 + num9;
				array[0] = new Vector2(num10, num11);
				array[1] = new Vector2(num10, y);
				array[2] = new Vector2(x, y);
				array[3] = new Vector2(x, num11);
			}
			vh.AddVert(vector5, currentColor, array[0]);
			vh.AddVert(vector2, currentColor, array[1]);
			vh.AddVert(vector3, currentColor, array[2]);
			vh.AddVert(vector4, currentColor, array[3]);
			vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}

		protected virtual void Update()
		{
			if (ParticleSystem != null && ParticleSystem.isPlaying)
			{
				SetVerticesDirty();
			}
			if (particleSystemRenderer != null && particleSystemRenderer.enabled)
			{
				particleSystemRenderer.enabled = false;
			}
		}
	}
}
