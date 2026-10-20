namespace PrintInventory.Forms;

// One choice in a ComboBox: the database Id plus the text the user sees.
// A ComboBox shows the result of ToString() for each item.
public class ListItem
{
    public int Id { get; }
    public string Text { get; }

    public ListItem(int id, string text)
    {
        Id = id;
        Text = text;
    }

    public override string ToString()
    {
        return Text;
    }

    // Selects the item with the given Id in a ComboBox filled with ListItems.
    public static void SelectById(ComboBox comboBox, int id)
    {
        foreach (ListItem item in comboBox.Items)
        {
            if (item.Id == id)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
    }
}
