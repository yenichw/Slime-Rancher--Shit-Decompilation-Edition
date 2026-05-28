using System.Collections.Generic;

public class FirestormController : SRBehaviour
{
	private FireColumn[] columns;

	public void Awake()
	{
		columns = GetComponentsInChildren<FireColumn>(includeInactive: true);
	}

	public void AddColumnsToList(List<FireColumn> nearbyColumns)
	{
		nearbyColumns.AddRange(columns);
	}
}
