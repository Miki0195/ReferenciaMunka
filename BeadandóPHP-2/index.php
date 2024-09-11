<?php
    session_start();
    include_once("./data/storage.php");

    function isCardInUserCollection(string $cardName, array $userCards): bool{
        return in_array($cardName, $userCards);
    }

    if(isset($_GET["logout"])){
        unset($_SESSION["user"]);
    }

    $cardsStorage = new Storage(new JsonIO("./data/cards.json"), true);
    $usersStorage = new Storage(new JsonIO("./data/users.json"), true);

    $user = isset($_SESSION["user"]) ? $usersStorage->findOne(["username" => $_SESSION["user"]]) : null;
    if($user == null && isset($_SESSION["user"])){
        unset($_SESSION["user"]);
    }
?>

<!DOCTYPE html>
<html lang="en">

    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>IKémon | Home</title>
        <link rel="stylesheet" href="./data/styles/main.css">
        <link rel="stylesheet" href="./data/styles/cards.css">
        <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"
            integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">
    </head>

    <body>
        <header>
            <h1><a href="index.php">IKémon</a> - Home</h1>
            <?php
            if(isset($_SESSION["user"])){
                echo '<h2 style="color: white;"><a href="user-details.php">' . $_SESSION["user"] . '</a></h2>';
            } 
            else{
                echo '<h2><a href="login.php">Bejelentkezés</a> | <a href="registration.php">Regisztráció</a></h2>';
            }
            if(isset($_SESSION["user"]) && $user["username"] !== "admin"){
                echo ': ' . $user["money"] . ' $' . '</h2>';
            } 
            else{
                echo '</h2>';
            }
            ?>
        </header>
        <div id="content">
            <form action="index.php" method="get">
                <label for="filter" class="form-label">Filter by Type</label>
                <select class="form-select" name="filter">
                    <option value="all">all</option>
                    <option value="normal">normal</option>
                    <option value="electric">electric</option>
                    <option value="fire">fire</option>
                    <option value="dragon">dragon</option>
                    <option value="grass">grass</option>
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
                <div class="mb-3 mt-3">
                    <input type="submit" value="Filter" class="btn btn-primary">
                </div>
            </form>
            <div id="card-list">
                <?php foreach($cardsStorage->findAll() as $card) : ?>
                    <?php 
                        if(isset($_GET["filter"]) && $_GET["filter"] !== "all" && $card["type"] !== $_GET["filter"]){
                            continue;
                        } 
                    ?>
                    <div class="pokemon-card">
                        <div class=<?php echo "clr-" . $card["type"] . " image" ?>>
                            <img src=<?php echo $card["image"] ?> alt="">
                        </div>
                        <div class="details">
                            <h2><a href=<?php echo "card-details.php?id=" . $card["id"] ?>><?php echo $card["name"] ?></a></h2>
                            <span class="card-type"><span class="icon">🏷</span> <?php echo $card["type"] ?></span>
                            <span class="attributes">
                                <span class="card-hp"><span class="icon">❤</span> <?php echo $card["hp"] ?></span>
                                <span class="card-attack"><span class="icon">⚔</span> <?php echo $card["attack"] ?></span>
                                <span class="card-defense"><span class="icon">🛡</span> <?php echo $card["defense"] ?></span>
                            </span>
                        </div>
                        <?php
                        if(isset($_SESSION["user"]) && $user["username"] !== "admin" && !isCardInUserCollection($card["name"], $user["cards"])){
                            if(isCardInUserCollection($card["name"], $usersStorage->findById("admin")["cards"])){
                                echo '<div class="buy"><span class="card-price"><a href="buy.php?name=' . $card["name"] . '"><span class="icon">💰</span> ';
                                echo $card["price"];
                                echo '</a></span></div>';
                            }
                        }
                        ?>
                    </div>
                <?php endforeach; ?>
            </div>
        </div>
        <footer>
            <p>IKémon | ELTE IK Webprogramozás</p>
        </footer>
    </body>

</html>
