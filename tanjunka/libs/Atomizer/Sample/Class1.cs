using System;
using System.IO;
using Atomizer;
using System.Text;

namespace AtomizerSample
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//You should do more checking here, for now,
			//assume arg[0] is username,
			//and arg[1] is password,
			//and arg[2] is either typepad or blogger, default to blogger.
			//and arg[3] is a filename of an image to upload to typepad
			Console.WriteLine("");

			if (args.Length < 3)
			{
				Console.WriteLine("Usage: app.exe <username> <password> <proxy> [<endpoint> | livedoor | typepad | blogger] [image filename]");
				return;
			}

            string username = args[0];
			string password = args[1];
			string proxy    = args[2];

            string atomEndPoint = args[3];

            if (args.Length >= 4)
			{
                if (args[3].Equals("blogger"))
                {
                    atomEndPoint = "https://www.blogger.com/atom/";
                }
                else if (args[3].Equals("livedoor"))
                {
                    atomEndPoint = "http://blog.livedoor.com/atom";
                }
                else if (args[3].Equals("typepad"))
                {
                    atomEndPoint = "http://www.typepad.com/t/atom/weblog";
                }
            }

			string filename = "";

			if (args.Length >= 5)
			{
				//fourth (optional) arg is filename
				filename = args[4];
			}

			//First, connect to the endpoint
			//with the user's name and password.
			//Also pass in your app's name
			//and it's homepage url.
			//This creates a "base" Atom API which we can
			//later conver to TypePad or Blogger depending
			//on the endpoint we pass in!

			generatorType generator = new generatorType();
			generator.url = "http://www.winisp.net/dstewartms/atomizer";
			generator.Value = "AtomizerSample";
			generator.version = "1.7";

			Atom atom =
				Atom.Create(new Uri(atomEndPoint), generator, username, password);

			atom.Proxy = new System.Net.WebProxy(proxy);

			//get the user's services from the endpoint, these correspond to blogs
			service[] services = atom.GetServices();

			//typepad has categories on posts; blogger doesn't.
			//but you can pass one in and it won't hurt
			string category = "";

			//We'll do more with typepad extensions later ... for now, just get
			//the user's categories ...

			//note that this is how we check to see if we're typepad now if
			//we don't just create a typepad object directly.  Much more .NET-ish
			if (atom is TypePad)
			{
				TypePad tp = atom as TypePad;

				foreach (service service in services)
				{
					//is this a service.categories type?
					if (service.srvType.Equals(serviceType.categories))
					{
						//get a "typepadspecific" object to play with
						string[] cats = tp.GetCategories(service.postURL);

						Console.WriteLine("Categories on \"" + service.name + "\"");

						foreach (string cat in cats)
						{
							category = cat; //logically, this means we'll get the last one to use later
							Console.WriteLine("   " + cat);
						}

						Console.WriteLine("");
					}
				}
			}

			//walk the service list -- careful, if you have multiple blogs,
			//we're going to post to them all ... but that's ok, we'll delete it
			foreach (service service in services)
			{
				//is this a service.post type?
				if (service.srvType.Equals(serviceType.post))
				{
					//if so, post an entry!
					Console.WriteLine("------ post new entry ------");
					entryType entry = atom.PostBlogEntry(service.postURL, "My first post!", "Here it is!", category);
					Console.WriteLine("Your entry \"" + entry.title + "\" was posted to " + service.name + "!");

					string editURL = null;
					foreach (linkType link in entry.links)
					{
						if (link.srvType.Equals(serviceType.edit))
						{
							editURL = link.href;
						}
					}

					if (editURL != null)
					{
						// read that entry back
						Console.WriteLine("------ get old entry it ------");
						entryType oldEntry = atom.GetBlogEntry(editURL);
						Console.WriteLine("Your entry \"" + oldEntry.title + "\" was GETed from " + service.name + "!");

						Console.WriteLine("------ edit old entry it ------");
						entryType editEntry = atom.EditBlogEntry(editURL, "My firist post edited", "some edited body", category);
						Console.WriteLine("Your entry \"" + editEntry.title + "\" was edited from " + service.name + "!");

						//now, delete that puppy!
						Console.WriteLine("------ delete it ------");
						atom.DeleteBlogEntry(editURL);
						Console.WriteLine("Your entry \"" + editEntry.title + "\" was deleted from " + service.name + "!");
						Console.WriteLine("");
					}
				}

				if (service.srvType.Equals(serviceType.feed))
				{
					//here's how to read an atom feed, it's pretty easy.
					feedType feed = atom.GetFeed(service.postURL);

					Console.WriteLine("Entries on \"" + feed.title + "\"");

					foreach (entryType entry in feed.entries)
					{
						Console.WriteLine("   " + entry.title);
					}

					Console.WriteLine("");
				} //end feeds
			}//end each service

			//are we playing with TypePad?
			//if so, we can do more fun stuff
			if (atom is TypePad)
			{
				//hey, let's upload a file ...
				//note we're sending a photo but
				//we can send any filename/filetype ...
				//posting a photo is better done with
				//code shown below, which posts straight
				//to the photo gallery itself
				if (filename.Length > 0)
				{
					foreach (service service in services)
					{
						if (service.srvType.Equals(serviceType.upload))
						{
							FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
							TypePad typepad = (TypePad)atom;

							//let's give it a nice name via FileInfo.Name
							FileInfo fi = new FileInfo(filename);

							entryType entry = typepad.UploadMimeFile(
								service.postURL,
								fs,
								"image/jpeg",
								fi.Name);

							Console.WriteLine("You can download this file from:");
							foreach (linkType link in entry.links)
							{
								if (link.srvType.Equals(serviceType.alternate))
								{
									Console.WriteLine("   " + link.href);
								}
							}

							Console.WriteLine("");
						}
					}
				}

				//post to various lists at http://www.typepad.com/t/atom/lists
				//because this is a new endpoint, we need another atom object
				//And here is how to specifically create a typepad object
				TypePad tp = new TypePad(new Uri("http://www.typepad.com/t/atom/lists"),
											     	generator,
													username,
													password);

				//get the lists the user has configured
				service[] serviceLists = tp.GetServices();

				//walk the service list -- careful, if you have multiple blogs,
				//we're going to post to them all ...
				foreach (service serviceList in serviceLists)
				{
					//the problem is that typepad doesn't tell you which list is which!
					//you MUST present UI to the user to ask.
					//For this sample, though, we'll just assume "books" and pop one up there.
					//Other lists are very similar, just use different objects

					//make sure it's a posting service
					if (serviceList.srvType.Equals(serviceType.post))
					{
						TypePad.book book = new TypePad.book();

						book.author = "David Foster Wallace";
						book.title = "Infinite Jest";

						Console.WriteLine("Adding a book to " + serviceList.name);
						tp.PostToBookList(serviceList.postURL, book);
					}
				}

				Console.WriteLine("");

				if (filename.Length > 0)
				{
					//OK, let's post to a photo gallery now, if an image filename was specified
					//Again, a new endpoint, so we need another Atom
					TypePad tpPhoto = new TypePad(new Uri("http://www.typepad.com/t/atom/gallery"),
						generator,
						username,
						password);

					//get the lists the user has configured
					service[] servicePhotos = tpPhoto.GetServices();

					//walk the service list --
					//again, we really should ask the user which one he wants to post to first
					foreach (service servicePhoto in servicePhotos)
					{
						//make sure it's a posting service
						if (servicePhoto.srvType.Equals(serviceType.post))
						{
							TypePad.photo photo = new TypePad.photo();

							photo.title = "My pic";
							photo.image = System.Drawing.Image.FromFile(filename);

							Console.WriteLine("Adding a photo to " + servicePhoto.name);

							entryType entryPhoto = tpPhoto.PostToPhotos(servicePhoto.postURL, photo);

							//show the link to this photo
							Console.WriteLine("You can download this photo from:");
							foreach (linkType link in entryPhoto.links)
							{
								if (link.srvType.Equals(serviceType.alternate))
								{
									Console.WriteLine("   " + link.href);
								}
							}

							photo.image.Dispose(); //this let's the file loose for later
						}
					}

					/*
					 * //just for fun, let's also put it to service.upload, which can take any file
					//use the original endpoint's services
					foreach (service service in services)
					{
						//is this a service.upload type?
						if (service.srvType.Equals(serviceType.upload))
						{
							//get a "typepadspecific" object to play with
							TypePadSpecific.TypePad tpFile = new TypePadSpecific.TypePad();

							Console.WriteLine("Uploading file to " + service.name);
							FileStream fileStream = new FileStream(filename, FileMode.Open);
							tpFile.UploadMimeFile(atom, service.postURL, fileStream, "image/jpeg");
						}
					}
					*/

				} //end if photo filename
			} //end if typepad
		} //end main
	} //end class
} //end namespace


