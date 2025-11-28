namespace Recommend.API.Models
{
    public enum EnumRecommendType : int
    {
        Platform = 1,
        Friend = 2, //一度好友
        Foaf = 3, // Friend of a friend 二度好友
    }
}