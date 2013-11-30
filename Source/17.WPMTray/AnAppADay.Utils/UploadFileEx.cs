/***
 * Taken with much glee from:
 * http://www.codeproject.com/csharp/UploadFileEx.asp
 * THANKS madmik3!
 * */

using System;
using System.Web;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;

namespace AnAppADay.Utils
{

    public class UploadFile
	{

		public static string UploadFileEx(	string uploadfile, string url,
			string fileFormName, string contenttype,NameValueCollection querystring,
			CookieContainer cookies, string referer)
		{
            System.Net.ServicePointManager.Expect100Continue = false;

			if( (fileFormName== null) ||
				(fileFormName.Length ==0))
			{
				fileFormName = "file";
			}

			if( (contenttype== null) ||
				(contenttype.Length ==0))
			{
				contenttype = "application/octet-stream";
			}


			string postdata;
			postdata = "?";
			if (querystring!=null)
			{
				foreach(string key in querystring.Keys)
				{
					postdata+= key +"=" + querystring.Get(key)+"&";
				}
			}
			Uri uri = new Uri(url+postdata);


			string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
			HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
			webrequest.CookieContainer = cookies;
			webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
			webrequest.Method = "POST";
            webrequest.Referer = referer;


			// Build up the post message header
			StringBuilder sb = new StringBuilder();
			sb.Append("--");
			sb.Append(boundary);
			sb.Append("\r\n");
			sb.Append("Content-Disposition: form-data; name=\"");
			sb.Append(fileFormName);
			sb.Append("\"; filename=\"");
			sb.Append(Path.GetFileName(uploadfile));
			sb.Append("\"");
			sb.Append("\r\n");
			sb.Append("Content-Type: ");
			sb.Append(contenttype);
			sb.Append("\r\n");
			sb.Append("\r\n");			

			string postHeader = sb.ToString();
			byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

			// Build the trailing boundary string as a byte array
			// ensuring the boundary appears on a line by itself
			byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            using (FileStream fileStream = new FileStream(uploadfile, FileMode.Open, FileAccess.Read))
            {
                long length = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
                webrequest.ContentLength = length;

                using (Stream requestStream = webrequest.GetRequestStream())
                {

                    // Write out our post header
                    requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                    // Write out the file contents
                    byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        requestStream.Write(buffer, 0, bytesRead);

                    // Write out the trailing boundary
                    requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                }
            }
			WebResponse responce = webrequest.GetResponse();
            using (Stream s = responce.GetResponseStream())
            {
                StreamReader sr = new StreamReader(s);

                return sr.ReadToEnd();
            }
		}
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			CookieContainer cookies = new CookieContainer();
			//add or use cookies

			NameValueCollection querystring = new NameValueCollection();
			
			// simulate this form 
			//<form action ="http://localhost/test.php" method = POST>
			//<input type = text name = uname>
			//<input type = password name =passwd>
			//<input type = FILE name = uploadfile>
			//<input type=submit>


			querystring["uname"]="uname";
			querystring["passwd"]="snake3";

			string uploadfile;// set to file to upload

			uploadfile = "c:\\test.jpg";
			

			//everything except upload file and url can be left blank if needed
            /*
			UploadFileEx(uploadfile,"http://localhost/test.php","uploadfile", "image/pjpeg",
				querystring,cookies);
            */

				/*
				*
				contents of test.php
				<?php

				print_r($_REQUEST);

				$uploadDir = '%SOMEPATH';
				$uploadFile = $uploadDir . $_FILES['userfile']['name'];
				print "<pre>";
				if (move_uploaded_file($_FILES['userfile']['tmp_name'], $uploadFile))
				{
					print "File is valid, and was successfully uploaded. ";
				}
				else
				{
					print "Possible file upload attack!  Here's some debugging info:\n";
					print_r($_FILES);
				}
				print "</pre>";

				?>*/
		}
	}
}
