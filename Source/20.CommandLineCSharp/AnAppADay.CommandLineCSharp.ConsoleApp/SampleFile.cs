{
    //show a messagebox
    MessageBox.Show(args[0]);

    //download from a website
    string page = new WebClient().DownloadString(args[1]);
    Console.WriteLine(page);
    
    //wait for input
    Console.ReadLine();
}