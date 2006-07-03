using System;
using CookComputing.XmlRpc;

/*
blogger.newPost
Description: Creates a new post, and optionally publishes it.
Parameters:
 String appkey
 String blogid
 String username
 String password
 String content
 boolean publish

Return value: on success, String postid of new post; on failure, fault

*/

/*
blogger.editPost
Description: Updates the information about an existing post.
Parameters:
 String appkey
 String postid
 String username
 String password
 String content
 boolean publish

Return value: on success, boolean true value; on failure, fault


*/

/*
blogger.deletePost
Description: Deletes a post.
Parameters:
 String appkey
 String postid
 String username
 String password
 boolean publish

Return value: on success, boolean true value; on failure, fault


*/

/*
blogger.getRecentPosts
Description: Returns a list of the most recent posts in the system.
Parameters:
 String appkey
 String blogid
 String username
 String password
 int numberOfPosts

Return value: on success
 array of structs containing

 ISO.8601 dateCreated
 String userid
 String postid
 String content

 ; on failure fault

Notes: dateCreated is in the timezone of the weblog blogid

*/

public struct blogger_RecentPost
{
    public DateTime dateCreated;
    public String userid;
    public String postid;
    public String content;
};

public struct livejournal_RecentPost
{
    public DateTime dateCreated;
    public String userID;
    public String postId;
    public String content;
};

/*
blogger.getUsersBlogs
Description: Returns a list of weblogs to which an author has posting privileges.
Parameters:
 String appkey
 String username
 String password

Return value: on success
 array of structs containing
 String url
 String blogid
 String blogName

 ; on failure, fault
*/

public struct blogger_userBlog
{
    public string url;
    public string blogid;
    public string blogName;
};

/*
blogger.getUserInfo
Description: Returns information about an author in the system.
Parameters:
 String appkey
 String username
 String password

Return value: on success
 struct containing

 String userid
 String firstname
 String lastname
 String nickname
 String email
 String url

 ; on failure, fault

Notes: firstname is the Movable Type username up to the first space character
       and lastname is the username after the first space character.
*/

public struct blogger_userInfo
{
    public string userid;
    public string firstname;
    public string lastname;
    public string nickname;
    public string email;
    public string url;
};

// ***********************************  ************************************

/*
metaWeblog.newPost
Description: Creates a new post, and optionally publishes it.
Parameters:
 String blogid
 String username
 String password
 struct content
 boolean publish

Return value: on success
 String postid of new post; on failure, fault

Notes: the struct content can contain the following standard keys:
 title for the title of the entry
 description for the body of the entry
 dateCreated  to set the created-on date of the entry.

 In addition Movable Type's implementation allows you to pass
 in values for five other keys:

 int mt_allow_comments the value for the allow_comments field
 int mt_allow_pings the value for the allow_pings field
 String mt_convert_breaks the value for the convert_breaks field
 String mt_text_more the value for the additional entry text
 String mt_excerpt the value for the excerpt field
 String mt_keywords the value for the keywords field
 array mt_tb_ping_urls the list of TrackBack ping URLs for this entry.

 If specified dateCreated should be in ISO.8601 format.
*/

public struct metaWeblog_newPostInfo
{
    public string title;
    public string description;
    public DateTime dateCreated;

    public int mt_allow_comments;
    public int mt_allow_pings;
    public string mt_convert_breaks;
    public string mt_text_more;
    public string mt_excerpt;
    public string mt_keywords;
    public string [] mt_tb_ping_urls;
};


/*
metaWeblog.editPost
Description: Updates information about an existing post.
Parameters:
 String postid
 String username
 String password
 struct content
 boolean publish    // You can't set this to true if it was previous published!

Return value: on success, boolean true value; on failure, fault

Notes: the struct content can contain the following standard keys:
 title for the title of the entry
 description for the body of the entry
 dateCreated to set the created-on date of the entry.

 In addition Movable Type's implementation allows you to pass
 in values for five other keys:

 int mt_allow_comments the value for the allow_comments field
 int mt_allow_pings the value for the allow_pings field
 String mt_convert_breaks the value for the convert_breaks field
 String mt_text_more the value for the additional entry text
 String mt_excerpt the value for the excerpt field
 String mt_keywords the value for the keywords field
 array mt_tb_ping_urls the list of TrackBack ping URLs for this entry.

 If specified, dateCreated should be in ISO.8601 format.
*/

/*
metaWeblog.getPost
Description: Returns information about a specific post.
Parameters:
 String postid
 String username
 String password

Return value: on success
 struct containing

 String userid
 ISO.8601 dateCreated
 String postid
 String description
 String title
 String link
 String permaLink
 String mt_excerpt
 String mt_text_more
 int mt_allow_comments
 int mt_allow_pings
 String mt_convert_breaks
 String mt_keywords

 ; on failure, fault

Notes: link and permaLink are both the URL pointing to the archived post. The fields prefixed with mt_ are Movable Type extensions to the metaWeblog.getPost API.


*/
public struct metaWeblog_postInfo
{
//    private DateTime _dateCreated;
//    private bool _haveDate;

    public string userid;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public DateTime dateCreated;
//    public DateTime dateCreated { get { return (_haveDate ? _dateCreated : null); } set { _dateCreated = value; _haveDate = true; } }

    public string postid;

    public string description;
    public string title;
    public string link;
    public string permaLink;

    public string mt_excerpt;
    public string mt_text_more;
    public int mt_allow_comments;
    public int mt_allow_pings;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public string mt_convert_breaks;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public string mt_keywords;
};


/*
metaWeblog.getRecentPosts
Description: Returns a list of the most recent posts in the system.
Parameters: String blogid
 String username
 String password
 int numberOfPosts

Return value: on success
 array of structs containing ISO.8601 dateCreated
 String userid
 String postid
 String description
 String title
 String link
 String permaLink
 String mt_excerpt
 String mt_text_more
 int mt_allow_comments
 int mt_allow_pings
 String mt_convert_breaks
 String mt_keywords; on failure, fault

Notes: dateCreated is in the timezone of the weblog blogid; link and permaLink are the URL pointing to the archived post


*/

/*
metaWeblog.newMediaObject
Description: Uploads a file to your webserver.
Parameters:
 String blogid
 String username
 String password
 struct file

Return value: URL to the uploaded file.

Notes: the struct file should contain two keys:
 base64 bits (the base64-encoded contents of the file)
 String name (the name of the file).
 The type key (media type of the file) is currently ignored.
*/

public struct metaWeblog_file
{
    public byte [] bits; // XMLRPC::Data->type('base64', $bits),
    public string type; // "image/jpeg"
    public string name;
};

public struct metaWeblog_newObjInfo
{
    public string url;
};

/*
mt.getRecentPostTitles
Description: Returns a bandwidth-friendly list of the most recent posts in the system.
Parameters:
 String blogid
 String username
 String password
 int numberOfPosts

Return value: on success
 array of structs containing

 ISO.8601 dateCreated
 String userid
 String postid
 String title

 ; on failure, fault

Notes: dateCreated is in the timezone of the weblog blogid
*/

public struct mt_title
{
    public DateTime dateCreated;
    public string userid;
    public string postid;
    public string title;
};

/*
mt.getCategoryList
Description: Returns a list of all categories defined in the weblog.
Parameters:
 String blogid
 String username
 String password

Return value: on success
 an array of structs containing

 String categoryId
 String categoryName

 ; on failure, fault.
*/

public struct mt_category
{
    public string categoryId;
    public string categoryName;
};

/*
mt.getPostCategories
Description: Returns a list of all categories to which the post is assigned.
Parameters:
 String postid
 String username
 String password

Return value: on success
 an array of structs containing

 String categoryName
 String categoryId
 boolean isPrimary

 ; on failure, fault.

Notes: isPrimary denotes whether a category is the post's primary category.

*/

public struct mt_postCategory
{
    public string categoryId;
    public string categoryName;
    public bool   isPrimary;
};

/*
mt.setPostCategories
Description: Sets the categories for a post.
Parameters:
 String postid
 String username
 String password
 array categories

Return value: on success, boolean true value; on failure, fault

Notes: the array categories is an array of structs containing

 String categoryId
 boolean isPrimary. Using isPrimary to set the primary category is optional

 in the absence of this flag the first struct in the array will
 be assigned the primary category for the post.
*/

public struct mt_setCategory
{
    public string categoryId;
    public bool   isPrimary;
};

/*
mt.supportedMethods
Description: Retrieve information about the XML-RPC methods supported by the server.
Parameters: none

Return value: an array of method names supported by the server.
*/

/*
mt.supportedTextFilters
Description: Retrieve information about the text formatting plugins supported by the server.
Parameters: none

Return value: an array of structs containing
 String key
 String label.

 key is the unique string identifying a text formatting plugin
 and label is the readable description to be displayed to a user.
 key is the value that should be passed in the mt_convert_breaks parameter
 to newPost and editPost.
*/

public struct mt_textFilter
{
    public string key;
    public string label;
};

/*
mt.getTrackbackPings
Description:
 Retrieve the list of TrackBack pings posted to a particular entry.
 This could be used to programmatically retrieve the list of pings
 for a particular entry  then iterate through each of those pings doing the same
 until one has built up a graph of the web of entries referencing one another
 on a particular topic.

Parameters:
  String postid

Return value: an array of structs containing
 String pingTitle (the title of the entry sent in the ping)
 String pingURL (the URL of the entry)
 String pingIP (the IP address of the host that sent the ping).
*/

public struct mt_ping
{
    public string pingTitle;
    public string pingURL;
    public string pingIP;
};

/*
mt.publishPost
Description: Publish (rebuild) all of the static files related to an entry from your weblog. Equivalent to saving an entry in the system (but without the ping).
Parameters:
 String postid
 String username
 String password

Return value: on success, boolean true value; on failure, fault

*/


//[XmlRpcUrl("http://bogus.not/mt/mt-xmlrpc.cgi")]
#if false
interface MovabletypeAPI
{
    [XmlRpcMethod("blogger.newPost")]
    string blogger_newPost(string appkey, string blogid, string username, string password, string content, bool publish);

    [XmlRpcMethod("blogger.editPost")]
    bool blogger_editPost(string appkey, string blogid, string username, string password, string content, bool publish);

    [XmlRpcMethod("blogger.deletePost")]
    bool blogger_deletePost(string appkey, string postid, string username, string password, bool publish);

    [XmlRpcMethod("blogger.getRecentPosts")]
    blogger_RecentPost [] blogger_getRecentPosts(string appkey, string blogid, string username, string password, int numberOfPosts);

    [XmlRpcMethod("blogger.getUsersBlogs")]
    blogger_userBlog [] blogger_getUsersBlogs(string appkey, string username, string password);

    [XmlRpcMethod("blogger.getUserInfo")]
    blogger_userInfo blogger_getUserInfo(string appkey, string username, string password);

    /* --- */

    [XmlRpcMethod("metaWeblog.newPost")]
    string metaWeblog_newPost(string blogid, string username, string password, metaWeblog_newPostInfo content, bool publish);

    [XmlRpcMethod("metaWeblog.editPost")]
    bool metaWeblog_editPost(string postid, string username, string password, metaWeblog_newPostInfo content, bool publish);

    [XmlRpcMethod("metaWeblog.getPost")]
    metaWeblog_postInfo metaWeblog_getPost(string postid, string username, string password);

    [XmlRpcMethod("metaWeblog.getRecentPosts")]
    metaWeblog_postInfo [] metaWeblog_GetResentPosts(string blogid, string username, string password, int numberOfPosts);

    [XmlRpcMethod("metaWeblog.newMediaObject")]
    metaWeblog_newObjInfo metaWeblog_newMediaObject(string blogid, string username, string password, metaWeblog_file file);

    [XmlRpcMethod("metaWeblog.getRecentPostTitles")]
    metaWeblog_title [] metaWeblog_getRecentPostTitles(string blogid, string username, string password, int numberOfPosts);

    [XmlRpcMethod("metaWeblog.getCategoryList")]
    metaWeblog_category [] metaWeblog_getCategoryList(string blogid, string username, string password);

    [XmlRpcMethod("metaWeblog.getPostCategories")]
    metaWeblog_postCategory [] metaWeblog_getPostCategories(string postid, string username, string password);

    [XmlRpcMethod("metaWeblog.setPostCategories")]
    bool metaWeblog_gstPostCategories(string postid, string username, string password, metaWeblog_setCategory [] categories);

    [XmlRpcMethod("mt.supportedMethods")]
    string [] mt_supportedMethods();

    [XmlRpcMethod("mt.supportedTextFilters")]
    mt_textFilter [] mt_supportedTextFilters();

    [XmlRpcMethod("mt.getTrackbackPings")]
    mt_ping [] mt_getTrackbackPings(string postid);

    [XmlRpcMethod("mt.publishPost")]
    bool mt_publish(string postid, string username, string password);
}
#endif

public class BloggerAPIProxy : XmlRpcClientProtocol
{
    [XmlRpcMethod("blogger.newPost")]
    public string blogger_newPost(string appkey, string blogid, string username, string password, string content, bool publish)
    {
        return (string)Invoke("blogger_newPost", new Object[] {appkey, blogid, username, password, content, publish, });
    }

    [XmlRpcMethod("blogger.editPost")]
    public bool blogger_editPost(string appkey, string postid, string username, string password, string content, bool publish)
    {
        return (bool)Invoke("blogger_editPost", new Object[] { appkey, postid, username, password, content, publish, });
    }

    [XmlRpcMethod("blogger.editPost")]
    public int blogger_editPost_intResponse(string appkey, string postid, string username, string password, string content, bool publish)
    {
        return (int)Invoke("blogger_editPost_intResponse", new Object[] { appkey, postid, username, password, content, publish, });
    }

    [XmlRpcMethod("blogger.getRecentPosts")]
    public blogger_RecentPost[] blogger_getRecentPosts(string appkey, string blogid, string username, string password, int numberOfPosts)
    {
        return (blogger_RecentPost[])Invoke("blogger_getRecentPosts", new Object[] { appkey, blogid, username, password,numberOfPosts, });
    }

    [XmlRpcMethod("blogger.getRecentPosts")]
    public livejournal_RecentPost[] livejournal_getRecentPosts(string appkey, string blogid, string username, string password, int numberOfPosts)
    {
        return (livejournal_RecentPost[])Invoke("livejournal_getRecentPosts", new Object[] { appkey, blogid, username, password,numberOfPosts, });
    }

    [XmlRpcMethod("blogger.getUsersBlogs")]
    public blogger_userBlog [] blogger_getUsersBlogs(string appkey, string username, string password)
    {
        return (blogger_userBlog [])Invoke("blogger_getUsersBlogs", new Object[] { appkey, username, password, });
    }

    /* this is basically useless.  I need to be able to lookup by userid, not name and password :-(
    [XmlRpcMethod("blogger.getUserInfo")]
    public blogger_userInfo blogger_getUserInfo(string appkey, string username, string password)
    {
        return (blogger_userInfo)Invoke("blogger_getUserInfo", new Object[] {  appkey, username, password, });
    }
    */
}

public class MovabletypeAPIProxy : XmlRpcClientProtocol
{
    /* --- */

    [XmlRpcMethod("metaWeblog.newPost")]
    public string metaWeblog_newPost(string blogid, string username, string password, metaWeblog_newPostInfo content, bool publish)
    {
        return (string)Invoke("metaWeblog_newPost", new Object[] { blogid, username, password, content, publish, });
    }

    [XmlRpcMethod("metaWeblog.editPost")]
    public bool metaWeblog_editPost(string postid, string username, string password, metaWeblog_newPostInfo content, bool publish)
    {
        return (bool)Invoke("metaWeblog_editPost", new Object[] { postid, username, password, content, publish, });
    }

    [XmlRpcMethod("metaWeblog.getPost")]
    public metaWeblog_postInfo metaWeblog_getPost(string postid, string username, string password)
    {
        return (metaWeblog_postInfo)Invoke("metaWeblog_getPost", new Object[] { postid, username, password, });
    }

    [XmlRpcMethod("metaWeblog.newMediaObject")]
    public metaWeblog_newObjInfo metaWeblog_newMediaObject(string blogid, string username, string password, metaWeblog_file file)
    {
        return (metaWeblog_newObjInfo)Invoke("metaWeblog_newMediaObject", new Object[] { blogid, username, password, file, });
    }

    [XmlRpcMethod("mt.getCategoryList")]
    public mt_category [] mt_getCategoryList(string blogid, string username, string password)
    {
        return (mt_category [])Invoke("mt_getCategoryList", new Object[] { blogid, username, password, });
    }

    [XmlRpcMethod("mt.getPostCategories")]
    public mt_postCategory [] mt_getPostCategories(string postid, string username, string password)
    {
        return (mt_postCategory [])Invoke("mt_getPostCategories", new Object[] { postid, username, password, });
    }

    [XmlRpcMethod("mt.setPostCategories")]
    public bool mt_setPostCategories(string postid, string username, string password, mt_setCategory [] categories)
    {
        return (bool)Invoke("mt_setPostCategories", new Object[] { postid, username, password, categories, });
    }

    [XmlRpcMethod("mt.getRecentPostTitles")]
    public mt_title [] mt_getRecentPostTitles(string blogid, string username, string password, int numberOfPosts)
    {
        return (mt_title [])Invoke("mt_getRecentPostTitles", new Object[] { blogid, username, password, numberOfPosts, });
    }

    [XmlRpcMethod("mt.supportedTextFilters")]
    public mt_textFilter [] mt_supportedTextFilters()
    {
        return (mt_textFilter [])Invoke("mt_supportedTextFilters", new Object[] { });
    }

    [XmlRpcMethod("mt.publishPost")]
    public bool mt_publish(string postid, string username, string password)
    {
        return (bool)Invoke("mt_publish", new Object[] { postid, username, password, });
    }

    [XmlRpcMethod("mt.publishPost")]
    public int mt_publish_intResponse(string postid, string username, string password)
    {
        return (int)Invoke("mt_publish_intResponse", new Object[] { postid, username, password, });
    }

    [XmlRpcMethod("mt.getTrackbackPings")]
    public mt_ping [] mt_getTrackbackPings(string postid)
    {
        return (mt_ping [])Invoke("mt_getTrackbackPings", new Object[] { postid, });
    }

}

public struct gman_hash
{
    public string  id;
    public string  value;

    gman_hash(string in_id, string in_value)
    {
        id    = in_id;
        value = in_value;
    }
};

public struct gman_response
{
    public int         status;
    public string      errMessage;
    public gman_hash[] data;

    public string GetEntry(string id)
    {
        foreach (gman_hash e in data)
        {
            if (e.id.CompareTo(id) == 0)
            {
                return e.value;
            }
        }
        return "";
    }
};

public class GManAPIProxy : XmlRpcClientProtocol
{
    /* --- */

    [XmlRpcMethod("gman.newEntry")]
    public gman_response gman_newEntry(string blogid, string username, string password, gman_hash[] content)
    {
        return (gman_response)Invoke("gman_newEntry", new Object[] { blogid, username, password, content, });
    }

    [XmlRpcMethod("gman.editEntry")]
    public gman_response gman_editEntry(string postid, string username, string password, gman_hash[] content)
    {
        return (gman_response)Invoke("gman_editEntry", new Object[] { postid, username, password, content, });
    }

    [XmlRpcMethod("gman.getEntry")]
    public gman_response gman_getEntry(string postid, string username, string password)
    {
        return (gman_response)Invoke("gman_getEntry", new Object[] { postid, username, password, });
    }

    [XmlRpcMethod("metaWeblog.newMediaObject")]
    public metaWeblog_newObjInfo metaWeblog_newMediaObject(string blogid, string username, string password, metaWeblog_file file)
    {
        return (metaWeblog_newObjInfo)Invoke("metaWeblog_newMediaObject", new Object[] { blogid, username, password, file, });
    }

    [XmlRpcMethod("mt.getCategoryList")]
    public mt_category [] mt_getCategoryList(string blogid, string username, string password)
    {
        return (mt_category [])Invoke("mt_getCategoryList", new Object[] { blogid, username, password, });
    }

    [XmlRpcMethod("gman.getIconList")]
    public mt_category [] gman_getIconList(string blogid, string username, string password)
    {
        return (mt_category [])Invoke("gman_getIconList", new Object[] { blogid, username, password, });
    }

    [XmlRpcMethod("mt.getRecentPostTitles")]
    public mt_title [] mt_getRecentPostTitles(string blogid, string username, string password, int numberOfPosts)
    {
        return (mt_title [])Invoke("mt_getRecentPostTitles", new Object[] { blogid, username, password, numberOfPosts, });
    }

}


