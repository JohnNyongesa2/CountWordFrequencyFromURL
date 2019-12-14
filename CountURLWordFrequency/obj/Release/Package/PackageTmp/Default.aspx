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
    <div class="alert alert-danger">
  <span style="color:crimson"><asp:PlaceHolder ID = "ErrorPlaceHolder" runat="server" /></span>
</div>
    <br />
    <ul class="list-group">
       <asp:PlaceHolder ID = "WordsPlaceHolder" runat="server" />
</ul>
    <br />


   
</body>
</html>
