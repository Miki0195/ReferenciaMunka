<?php
    session_start();
    include_once("./data/storage.php");

    $storage = new Storage(new JsonIO("./data/users.json"), true);
    $cards = new Storage(new JsonIO("./data/cards.json"), true);
    $current = null;

    if(isset($_SESSION["user"])){
        $current = $storage->findOne(["username" => $_SESSION["user"]]);
    }

    if($current == null){
        die("Hozzáférés megtagadva!");
    }

    $success = false;
    $errors = ["Név kötelező!", "Nem megfelelő támadási szint!", "Nem megfelelő védekezési szint!", "Nem megfelelő ár!", "Nem megfelelő HP!", "Leírás szükséges!", "IKémon ID kötelező!"];

    if($_SERVER["REQUEST_METHOD"] == "POST"){
        $validName = isset($_POST["name"]) && strlen($_POST["name"]) > 0;
        $validDescription = isset($_POST["desc"]) && strlen($_POST["desc"]) > 0;
        $validHp = isset($_POST["hp"]) && $_POST["hp"] > 0;
        $validAttack = isset($_POST["attack"]) && $_POST["attack"] > 0;
        $validDefense = isset($_POST["defense"]) && $_POST["defense"] > 0;
        $validPrice = isset($_POST["price"]) && $_POST["price"] > 0;
        $validId = isset($_POST["img"]) && $_POST["img"] > 0;

        if ($validName && $validDescription && $validHp && $validAttack && $validDefense && $validPrice && $validId){
            $success = true;
            $errors = null;

            $rec = [
                "name" => $_POST["name"],
                "type" => $_POST["type"],
                "hp" => $_POST["hp"],
                "attack" => $_POST["attack"],
                "defense" => $_POST["defense"],
                "price" => $_POST["price"],
                "description" => $_POST["desc"],
                "image" => $_POST["img"] < 100 ? "https://assets.pokemon.com/assets/cms2/img/pokedex/full/0" . $_POST["img"] . ".png" : null
            ];

            $cards->add($rec);
            array_push($current["cards"], $rec["name"]);
            $storage->update("admin", $current);
            header("Location: {$_SERVER['PHP_SELF']}");
            exit();
        } 
        else{
            if($validName) unset($errors[0]);
            if($validAttack) unset($errors[1]);
            if($validDefense) unset($errors[2]);
            if($validPrice) unset($errors[3]);
            if($validHp) unset($errors[4]);
            if($validDescription) unset($errors[5]);
            if($validId) unset($errors[6]);
        }
    }

    $nameValue = isset($_POST['name']) ? htmlspecialchars($_POST['name']) : '';
    $hpValue = isset($_POST['hp']) ? htmlspecialchars($_POST['hp']) : '';
    $attackValue = isset($_POST['attack']) ? htmlspecialchars($_POST['attack']) : '';
    $defenseValue = isset($_POST['defense']) ? htmlspecialchars($_POST['defense']) : '';
    $priceValue = isset($_POST['price']) ? htmlspecialchars($_POST['price']) : '';
    $descValue = isset($_POST['desc']) ? htmlspecialchars($_POST['desc']) : '';
    $imgValue = isset($_POST['img']) ? htmlspecialchars($_POST['img']) : '';
?>

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?php echo "IKémon | " . $_SESSION["user"] ?></title>
    <link rel="stylesheet" href="./data/styles/main.css">
    <link rel="stylesheet" href="./data/styles/cards.css">
    <link rel="stylesheet" href="./data/styles/details.css">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">
    <style>
        #contents {
            max-width: 1200px;
            margin: auto;
            padding: 20px;
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            margin-top: 20px;
        }

        #detailss {
            margin-bottom: 30px;
        }

        #detailss h2 {
            color: #007bff;
            margin-bottom: 15px;
        }

        .description {
            margin-bottom: 10px;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }

        th, td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }

        th {
            background-color: #007bff;
            color: #fff;
        }

        tbody tr:nth-child(even) {
            background-color: #f2f2f2;
        }

        tbody tr:hover {
            background-color: #ddd;
        }

        input[type="submit"] {
            background-color: #28a745;
            color: #fff;
            border: none;
            padding: 8px 15px;
            cursor: pointer;
        }

        input[type="submit"]:hover {
            background-color: #218838;
        }

        .card-list {
            display: flex;
            flex-wrap: wrap;
            justify-content: space-around;
        }

        .pokemon-card {
            width: 300px;
            margin: 15px;
            border: 1px solid #ddd;
            border-radius: 8px;
            overflow: hidden;
        }

        .image img {
            width: 100%;
            height: auto;
        }

        .details {
            padding: 15px;
        }

        .buy {
            text-align: center;
            padding: 15px;
        }

        .buy a {
            text-decoration: none;
            color: #28a745;
            font-weight: bold;
        }

        .buy a:hover {
            text-decoration: underline;
        }

        #card-list hr {
            border: 4px solid white;
            margin: 30px 0;
        }

        form {
            margin-top: 20px;
        }

        .form-label {
            font-weight: bold;
        }

        .form-control {
            width: 100%;
            padding: 8px;
            margin-bottom: 15px;
            border: 1px solid #ddd;
            border-radius: 4px;
            box-sizing: border-box;
        }

        .form-select {
            width: 100%;
            padding: 8px;
            margin-bottom: 15px;
            border: 1px solid #ddd;
            border-radius: 4px;
            box-sizing: border-box;
        }

        .btn-primary {
            background-color: #007bff;
            color: #fff;
            border: none;
            padding: 8px 15px;
            cursor: pointer;
            border-radius: 4px;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }
    </style>
</head>

<body>
    <header>
        <h1><a href="index.php">IKémon</a> - <?php echo $_SESSION["user"] ?></h1>
        <h2><a href="index.php?logout=1">Kijelentkezés</a></h2>
    </header>
    <div id="contents">
        <div id="detailss">
            <h2>Felhasználó információ:</h2>
            <div class="description">Felhasználónév: <?php echo $current["username"] ?></div>
            <div class="description">E-Mail: <?php echo $current["email"] ?></div>
            <div class="description">Pénz: <?php echo $current["money"] ?></div>
            <div class="description">Birtokolt kártyák száma: <?php echo count($current["cards"]) ?></div>
        </div>
        <hr style="border: 4px solid white;">
        <div id="card-list">
            <?php foreach ($cards->findMany(function ($var) use ($current) {
                $in = false;
                foreach ($current["cards"] as $curr) {
                    if (strcmp($var["name"], $curr) == 0) {
                        $in = true;
                        break;
                    }
                }
                return $in;
            }) as $c) : ?>
                <div class="pokemon-card">
                    <div class=<?php echo "clr-" . $c["type"] . " image" ?>>
                        <img src=<?php echo $c["image"] ?>>
                    </div>
                    <div class="details">
                        <h2><a href=<?php echo "card-details.php?id=" . $c["id"] ?>><?php echo $c["name"] ?></a></h2>
                        <span class="card-type"><span class="icon">🏷</span> <?php echo $c["type"] ?></span>
                        <span class="attributes">
                            <span class="card-hp"><span class="icon">❤</span> <?php echo $c["hp"] ?></span>
                            <span class="card-attack"><span class="icon">⚔</span> <?php echo $c["attack"] ?></span>
                            <span class="card-defense"><span class="icon">🛡</span> <?php echo $c["defense"] ?></span>
                        </span>
                    </div>
                    <?php
                    if (strcmp($current["username"], "admin") != 0) {
                        echo '<div class="buy"><span class="card-price"><a href="sell.php?name=' . $c["name"] . '">';
                        echo $c["price"] * 0.9 . ' $</a>';
                        echo '</span></div>';
                    }
                    ?>
                </div>
            <?php endforeach; ?>
        </div>
        <?php if (strcmp($current["username"], "admin") == 0) : ?>
            <hr style="border: 4px solid white;">
            <h2>Addj hozzá új kártyát!</h2>
            <ul>
                <?php
                if ($_SERVER["REQUEST_METHOD"] == "POST" && $errors != null)
                    foreach ($errors as $e) : ?>
                        <li><?php echo $e ?></li>
                <?php endforeach; ?>
            </ul>
            <form action="user-details.php" method="post">
                <div class="mb-3 mt-3">
                    <label for="name" class="form-label">Card Name</label>
                    <input type="text" class="form-control" name="name" value="<?php echo $nameValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <label for="type" class="form-label">Type</label>
                    <select name="type" class="form-select">
                    <option value="normal">normal</option>
                        <option value="electric">electric</option>
                        <option value="fire">fire</option>
                        <option value="dragon">dragon</option>
                        <option value="graas">graas</option>
                        <option value="poison">poison</option>
                        <option value="bug">bug</option>
                        <option value="dark">dark</option>
                        <option value="fairy">fairy</option>
                        <option value="fighting">fighting</option>
                        <option value="flying">flying</option>
                        <option value="psychic">psychic</option>
                        <option value="steel">steel</option>
                        <option value="water">water</option>
                        <option value="ice">ice</option>
                        <option value="ground">ground</option>
                        <option value="rock">rock</option>
                    </select>
                </div>
                <div class="mb-3 mt-3">
                    <label for="hp" class="form-label">HP</label>
                    <input type="number" name="hp" class="form-control" value="<?php echo $hpValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <label for="attack" class="form-label">Attack</label>
                    <input type="number" name="attack" class="form-control" value="<?php echo $attackValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <label for="defense" class="form-label">Defense</label>
                    <input type="number" name="defense" class="form-control" value="<?php echo $defenseValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <label for="price" class="form-label">Price</label>
                    <input type="number" name="price" class="form-control" value="<?php echo $priceValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <label for="desc" class="form-label">Description</label>
                    <input type="textarea" name="desc" class="form-control" value="<?php echo $descValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <label for="img" class="form-label">IKémon Number</label>
                    <input type="number" name="img" class="form-control" value="<?php echo $imgValue; ?>">
                </div>
                <div class="mb-3 mt-3">
                    <button type="submit" class="btn btn-primary">Hozzáad</button>
                </div>
            </form>

        <?php endif; ?>
    </div>
</body>

</html>
