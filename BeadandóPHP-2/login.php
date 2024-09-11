<?php
    session_start();
    include_once("./data/storage.php");

    $storage = new Storage(new JsonIO("./data/users.json"), true);

    $errorMessages = ["Felhasználónév kötelező", "Jelszó kötelező!", "Hibás felhasználónév vagy jelszó"];
    $errors = [];

    if($_SERVER["REQUEST_METHOD"] == "POST"){
        $username = $_POST["username"] ?? "";
        $password = $_POST["password"] ?? "";

        $errors[0] = !empty($username);
        $errors[1] = !empty($password);

        if($errors[0] && $errors[1]){
            $user = $storage->findOne(["username" => $username]);

            $errors[2] = $user !== null && $user["password"] === $password;

            if($errors[2]){
                $_SESSION["user"] = $username;
                header("Location: ./index.php");
                exit();
            }
        }
}
?>

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>IKémon | Regisztráció</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">
    <style>
        body {
            background-color: #f8f9fa;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .container {
            max-width: 400px;
            margin: auto;
            padding: 20px;
            margin-top: 50px;
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }

        header {
            text-align: center;
            margin-bottom: 20px;
        }

        .custom-form {
            display: flex;
            flex-direction: column;
        }

        .btn-custom {
            background-color: #4caf50;
            color: #fff;
            border: none;
            padding: 10px 15px;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn-custom:hover {
            background-color: #45a049;
        }

        .error-list {
            color: red;
            margin-top: 10px;
            padding-left: 0;
            list-style: none;
        }
    </style>
</head>

<body>
    <div class="container">
        <header>
            <h1><a href="index.php">IKémon</a> - Bejelentkezés</h1>
        </header>
        <ul class="error-list">
                <?php foreach($errors as $key => $value) : ?>
                    <?php if(!$value) echo "<li>{$errorMessages[$key]}</li>"; ?>
                <?php endforeach; ?>
            </ul>
        <div id="content">
            <form action="login.php" method="post" class="custom-form">
                <div class="mb-3">
                    <label for="username" class="form-label">Felhasználónév</label>
                    <input type="text" class="form-control" name="username">
                </div>
                <div class="mb-3">
                    <label for="password" class="form-label">Jelszó</label>
                    <input type="password" class="form-control" name="password">
                </div>
                <div class="mb-3">
                    <button type="submit" class="btn btn-custom">Bejelentkezés</button>
                </div>
            </form>
        </div>
        <footer>
            <p>IKémon | ELTE IK Webprogramozás</p>
        </footer>
    </div>
</body>

</html>
