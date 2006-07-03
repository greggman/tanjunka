using System;
using CookComputing.XmlRpc;

namespace CookComputing.MetaWeblog
{
  // TODO: following attribute is a temporary workaround
  [XmlRpcMissingMapping(MappingAction.Ignore)]
  struct Enclosure
  {
    public int length;
    public string type;
    public string url;
  }

  // TODO: following attribute is a temporary workaround
  [XmlRpcMissingMapping(MappingAction.Ignore)]
  public struct Source
  {
    public string name;
    public string url;
  }

  [XmlRpcMissingMapping(MappingAction.Ignore)]
  struct Post
  {
    [XmlRpcMissingMapping(MappingAction.Error)]
    [XmlRpcMember(Description="Required when posting.")]
    public DateTime dateCreated;
    [XmlRpcMissingMapping(MappingAction.Error)]
    [XmlRpcMember(Description="Required when posting.")]
    public string description;
    [XmlRpcMissingMapping(MappingAction.Error)]
    [XmlRpcMember(Description="Required when posting.")]
    public string title;

    public string[] categories;
    public Enclosure enclosure;
    public string link;
    public string permalink;
    [XmlRpcMember(
      Description="Not required when posting. Depending on server may "
      + "be either string or integer. "
      + "Use Convert.ToInt32(postid) to treat as integer or "
      + "Convert.ToString(postid) to treat as string")]
    public object postid;       
    public Source source;
    public string userid;

    public bool mt_allow_comments;
    public bool mt_allow_pings;
    public bool mt_convert_breaks;
    public string mt_text_more;
    public string mt_excerpt;
  }

  struct CategoryInfo
  {
    public string description;
    public string htmlUrl;
    public string rssUrl;
    public string title;
    public string categoryid;
  }

  struct Category
  {
    public string categoryId;
    public string categoryName;
  }

  interface IMetaWeblog
  {
    [XmlRpcMethod("metaWeblog.editPost",
      Description="Updates and existing post to a designated blog "
      + "using the metaWeblog API. Returns true if completed.")]
    bool editPost(
      string postid,
      string username,
      string password,
      Post post,
      bool publish);

    [XmlRpcMethod("metaWeblog.getCategories",
      Description="Retrieves a list of valid categories for a post "
      + "using the metaWeblog API. Returns the metaWeblog categories "
      + "struct collection.")]
    CategoryInfo[] getCategories(
      string blogid,
      string username,
      string password);

    [XmlRpcMethod("metaWeblog.getPost",
      Description="Retrieves an existing post using the metaWeblog "
      + "API. Returns the metaWeblog struct.")]
    Post getPost(
      string postid,
      string username,
      string password);

    [XmlRpcMethod("metaWeblog.getRecentPosts",
      Description="Retrieves a list of the most recent existing post "
      + "using the metaWeblog API. Returns the metaWeblog struct collection.")]
    Post[] getRecentPosts(
      string blogid,
      string username,
      string password,
      int numberOfPosts);

    [XmlRpcMethod("metaWeblog.newPost",
      Description="Makes a new post to a designated blog using the "
      + "metaWeblog API. Returns postid as a string.")]
    string newPost(
      string blogid,
      string username,
      string password,
      Post post,
      bool publish);
  }
}
