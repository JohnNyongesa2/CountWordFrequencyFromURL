<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CountURLWordFrequency.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css">
  <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
  <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js"></script>
</head>

<body>


    <br />
      <br />


    <div class="container">
  <ul class="nav nav-tabs">
    <li class="active"><a data-toggle="tab" href="#home">Fetch Words</a></li>
    <li><a data-toggle="tab" href="#menu1">Administrator (View Saved Words)</a></li>
   
  </ul>

  <div class="tab-content">
    <div id="home" class="tab-pane fade in active">
     <br />
      <br />


           <form id="form1" runat="server">
        <div>
            <div class="container">
  
  <p> This application fetches a page and builds a dictionary that contains the frequency of use of
each word on that page.</p>
  <form>
    <div class="form-group">
      <label for="usr">Enter the URL to the Page:</label>
      <input type="text" class="form-control" id="PageURL" name="PageURL" placeholder="e.g https://www.bbc.com/news">
      <span style="color:crimson"><asp:PlaceHolder ID = "PageURLDescriptionPlaceHolder" runat="server" /></span>
    </div>
      <asp:LinkButton Text="Fetch word frequency" runat="server" ID="submit" CssClass="btn btn-primary" OnClick="ButtonCount_Click"></asp:LinkButton>
  </form>
</div>

        </div>
    </form>
    <br />
    <div class="alert alert-info">
  <span style="color:crimson"><asp:PlaceHolder ID = "ErrorPlaceHolder" runat="server" /></span>
</div>
    <br />
    <ul class="list-group">
       <asp:PlaceHolder ID = "WordsPlaceHolder" runat="server" />
</ul>
    <br />

   </div>
    <div id="menu1" class="tab-pane fade">
         <br />
      <br />

      <p>Words saved in Databse, ordered by frequency of usage.</p>

         <table class="table">
    <thead>
      <tr>
        <th>Decryted Word</th>
        <th>Usage Frequency</th>
      </tr>
    </thead>
    <tbody>
           <asp:PlaceHolder ID = "SavedWordsPlaceHolder" runat="server" />
   
      
    </tbody>
  </table>

         <br /><br />
    <div class="alert alert-info">
  <span style="color:crimson"><asp:PlaceHolder ID = "ErrorPlaceHolder2" runat="server" /></span>
</div>
       
    </div>

   
  </div>
</div>


 


   
</body>
</html>
