using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.PersistenceLayer.Pagings.Paging;

public class Paginate<T>
{
    public Paginate()
    {
        Items = [];
    }

    [Required]
    [Description("Items per page.")]
    public int Size { get; set; }

    [Required]
    [Description("Zero-based number of this page.")]
    public int Index { get; set; }

    [Required]
    [Description("Items across all pages.")]
    public int Count { get; set; }

    [Required]
    [Description("Number of pages.")]
    public int Pages { get; set; }

    [Required]
    [Description("This page's items.")]
    public IList<T> Items { get; set; }

    [Description("Whether a page comes before this one.")]
    public bool HasPrevious => Index > 0;

    [Description("Whether a page comes after this one.")]
    public bool HasNext => Index + 1 < Pages;
}
