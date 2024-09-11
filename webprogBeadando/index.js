const missions = [
    {
        id: 0,
        title: "Az erdő széle",
        description: "A térképed szélével szomszédos erdőmezőidért egy-egy pontot kapsz."
      },
      {
        id: 1,
        title: "Álmos-völgy",
        description: "Minden olyan sorért, amelyben három erdőmező van, négy-négy pontot kapsz."
      },
      {
        id: 2,
        title: "Krumpliöntözés",
        description: "A farmmezőiddel szomszédos vízmezőidért két-két pontot kapsz."
      },
      {
        id: 3,
        title: "Határvidék",
        description: "Minden teli sorért vagy oszlopért 6-6 pontot kapsz."
      }
]

const hills = [
    {
        row: 2,
        col: 2
    },
    {
        row: 4,
        col: 9
    },
    {
        row: 6,
        col: 4
    },
    {
        row: 9,
        col: 10
    },
    {
        row: 10,
        col: 6
    }
]
const elements = [
    {
        time: 2,
        type: 'water',
        shape: [[1,1,1],
                [0,0,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false
    },
    {
        time: 2,
        type: 'town',
        shape: [[1,1,1],
                [0,0,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false        
    },
    {
        time: 1,
        type: 'forest',
        shape: [[1,1,0],
                [0,1,1],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'farm',
        shape: [[1,1,1],
                [0,0,1],
                [0,0,0]],
            rotation: 0,
            mirrored: false  
        },
    {
        time: 2,
        type: 'forest',
        shape: [[1,1,1],
                [0,0,1],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'town',
        shape: [[1,1,1],
                [0,1,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'farm',
        shape: [[1,1,1],
                [0,1,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 1,
        type: 'town',
        shape: [[1,1,0],
                [1,0,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 1,
        type: 'town',
        shape: [[1,1,1],
                [1,1,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 1,
        type: 'farm',
        shape: [[1,1,0],
                [0,1,1],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 1,
        type: 'farm',
        shape: [[0,1,0],
                [1,1,1],
                [0,1,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'water',
        shape: [[1,1,1],
                [1,0,0],
                [1,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'water',
        shape: [[1,0,0],
                [1,1,1],
                [1,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'forest',
        shape: [[1,1,0],
                [0,1,1],
                [0,0,1]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'forest',
        shape: [[1,1,0],
                [0,1,1],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
    {
        time: 2,
        type: 'water',
        shape: [[1,1,0],
                [1,1,0],
                [0,0,0]],
        rotation: 0,
        mirrored: false  
    },
]

document.getElementById("game-board").addEventListener("click", place, false)
document.getElementById("rotate").addEventListener("click", rotate, false)
document.getElementById("mirror").addEventListener("click", mirror, false)


let board = [
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0]
]

let points = [0,0,0,0]
let missionPoints = [0,0,0,0]

let currentSeason = {
    id: 0,
    name: "Tavasz"
}

let timeLeft = [7,7,7,7]
let tile
let missionsArr = []

function generateMissions(){
    let mission = document.getElementById("missions").children

    Array.from({ length: 2 }).forEach((_, i) => {
        Array.from({ length: 2 }).forEach((_, j) => {
            let missionId = generateRandomMission()
            missionsArr.push(missionId)
            mission[i + 1].children[j].innerHTML = `<img src="./images/mission${missions[missionId].id}.png">`
        })
    })
}

function generateRandomMission(){
    let missionId = Math.floor(Math.random()*missions.length)

    while(missionsArr.includes(missionId)){
        missionId = Math.floor(Math.random()*missions.length)
    }

    return missionId
}

function seasonChanging(){
    missionPoints[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id])
    console.log(missionPoints)
    switch(currentSeason.id){
        case 0: 
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id]].innerHTML = missions[missionsArr[currentSeason.id]].title + ": " + missionPoints[currentSeason.id]
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id + 1])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id + 1]].innerHTML = missions[missionsArr[currentSeason.id + 1]].title + ": " + missionPoints[currentSeason.id + 1]
            break
        case 1: 
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id]].innerHTML = missions[missionsArr[currentSeason.id]].title + ": " + missionPoints[currentSeason.id]
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id + 1])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id + 1]].innerHTML = missions[missionsArr[currentSeason.id + 1]].title + ": " + missionPoints[currentSeason.id + 1]
            break
        case 2: 
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id]].innerHTML = missions[missionsArr[currentSeason.id]].title + ": " + missionPoints[currentSeason.id]
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id + 1])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id + 1]].innerHTML = missions[missionsArr[currentSeason.id + 1]].title + ": " + missionPoints[currentSeason.id + 1]
            break
        case 3: 
            points[currentSeason.id] += calculatePoints(missionsArr[currentSeason.id])
            document.getElementById("mission-points").children[missionsArr[currentSeason.id]].innerHTML = missions[missionsArr[currentSeason.id]].title + ": " + missionPoints[currentSeason.id]
            points[currentSeason.id] += calculatePoints(missionsArr[0])
            document.getElementById("mission-points").children[missionsArr[0]].innerHTML = missions[0].title + ": " + missionPoints[0]
            break
        default:
            break
    }
    
    document.getElementById("season_points").children[currentSeason.id].innerHTML = currentSeason.name + ":<br> " + points[currentSeason.id] + " pont"
    let sum = points.reduce((acc, curr) => acc + curr, 0);
    document.getElementById("sum-points").innerHTML = "Összpontszám: " + sum

    currentSeason.id++;

    let missionPictures = document.getElementsByTagName("IMG")
    for(let i = 0; i < missionPictures.length; i++){
        missionPictures[i].classList = ""
    }
    switch(currentSeason.id){
        case 1:
            currentSeason.name = "Nyár"
            missionPictures[1].classList.add("active")
            missionPictures[2].classList.add("active")
            return
        case 2:
            currentSeason.name = "Ősz"
            missionPictures[2].classList.add("active")
            missionPictures[3].classList.add("active")
            return
        case 3:
            currentSeason.name = "Tél"
            missionPictures[3].classList.add("active") 
            missionPictures[0].classList.add("active")
            return
        default:
            currentSeason.id = 5
            currentSeason.name = "Vége"
            points[3] += hillSurrounded()
            return
        }
}

function drawTile(){

    const rows = document.getElementById("current-tile").children

    for (let i = 0; i < tile.shape.length; i++) {
        rows[i].innerHTML = tile.shape[i].map(cell => {
            return cell === 1
                ? '<div class="cell ' + tile.type + '"></div>'
                : '<div class="cell"></div>'
        }).join('')
    }
}

function rotate(){
    for(let i = 0; i < tile.shape.length; i++){
        for(let j = i + 1; j < tile.shape[i].length; j++){
            [tile.shape[i][j], tile.shape[j][i]] = [tile.shape[j][i], tile.shape[i][j]];
        }
    }
    for (let i = 0; i < tile.shape.length; i++) {
        tile.shape[i].reverse();
    }

    drawTile()
}

function mirror(){
    for(let i = 0; i < tile.shape.length; i++){
        [tile.shape[i][0], tile.shape[i][2]] = [tile.shape[i][2], tile.shape[i][0]]
    }

    drawTile()
}

function update(){
    let rows = document.getElementsByClassName("my-row")
    for(let i = 1; i <= board.length; i++){
        for(let j = 0; j < board[0].length; j++){
            rows[i].children[j].className = "cell"
            switch(board[i-1][j]){
                case 1:
                    rows[i].children[j].classList.add("hill")
                    break
                case 2:
                    rows[i].children[j].classList.add("forest")
                    break
                case 3:
                    rows[i].children[j].classList.add("water")
                    break
                case 4:
                    rows[i].children[j].classList.add("town")
                    break    
                case 5:
                    rows[i].children[j].classList.add("farm")
                    break
                default:
                    break
            }
        }
    }
    if(tile != undefined){
        timeLeft[currentSeason.id] -= tile.time
    }
    tile = elements[Math.floor(Math.random()*elements.length)]
    if(document.getElementById("remaining-time").innerHTML != "Évszakokból hátralévő idő: Vége"){
        drawTile()
    }
    if(timeLeft[currentSeason.id] == 0 || (timeLeft[currentSeason.id] == 1 && tile.time == 2)){
        seasonChanging()
    }
    if(currentSeason.id < 5){
        document.getElementById("remaining-time").innerHTML = "Évszakokból hátralévő idő: " + timeLeft[currentSeason.id] + "/7"
    }
    else{
        document.getElementById("remaining-time").innerHTML = "Évszakokból hátralévő idő: Vége"
        document.getElementById("current-tile").innerHTML = '<div></div>'
        document.querySelector("#game-board").removeEventListener("click",place,false)
    }

    document.getElementById("current-season").innerHTML = "Jelenlegi évszak: " + currentSeason.name
    document.getElementById("actual-tile-time").innerHTML = "Idő: " + tile.time
}

function getPosition(e){
    let rows = document.getElementsByClassName("my-row")
    for(let i = 1; i <= board.length; i++){
        for(let j = 0; j < board[0].length; j++){
            if(rows[i].children[j] == e.target)
            return [i-1,j]
        }
    }
}

function validPosition(position){
    const [row, col] = position
    for (let i = 0; i < tile.shape.length; i++) {
        for (let j = 0; j < tile.shape[i].length; j++) {
            let cellCords = tile.shape[i][j]
            if(cellCords == 1 && ((row + i < 0 || row + i >= board.length || col + j < 0 || col + j >= board[0].length))){
                alert("Hibás lerakás!")
                return false;
            }
            if (cellCords == 1 && board[row + i][col + j] !== 0) {
                alert("Hibás lerakás!")
                return false;
            }
        }
    }
    return true
}

function place(e){
    let pos = getPosition(e)
    if(!validPosition(pos)){
        return
    }
    for(let i = 0; i < tile.shape.length; i++){
        for(let j = 0; j < tile.shape[i].length; j++){
            if(tile.shape[i][j] == 1){
                board[pos[0] + i][pos[1] + j] = convert(tile.type)
            }
        }
    }
    update()
}

function convert(type){
    switch(type){
        case "forest":
            return 2
        case "water":
            return 3
        case "town":
            return 4
        case "farm":
            return 5
        default:
            return 0
    }
}

function calculatePoints(id){
    switch(id){
        case 0:
            return erdoSzele()
        case 1:
            return almosVolgy()
        case 2:
            return krumpliOntozes()
        case 3:
            return hatarVidek()
        default:
            return 0
    }
}

function erdoSzele(){
    let points = 0
    board[0].forEach(cell => {
        if (cell === 2) {
            points++
        }
    })
    
    board[board.length - 1].forEach(cell => {
        if (cell === 2) {
            points++
        }
    })

    for(let i = 1; i < board.length - 1; i++){
        if(board[i][0] == 2){
            points++
        }
        if(board[i][board[i].length - 1] == 2){
            points++;
        }
    }
    return points
}
function almosVolgy(){
    return board.filter(x => x.filter(k => k == 2).length == 3).length * 4
}
function krumpliOntozes(){
    let points = 0
    for(let i = 0; i < board.length; i++){
        for(let j = 0; j < board[0].length; j++){
            if(board[i][j] == 5){
                if(i-1 >= 0){
                    if(board[i-1][j] == 3){
                        points += 2
                    } 
                }
                if(i+1 < board.length){
                    if(board[i+1][j] == 3){
                        points += 2
                    } 
                }
                if(j-1 >= 0){
                    if(board[i][j-1] == 3){
                        points += 2
                    } 
                }
                if(j+1 < board[i].length){
                    if(board[i][j+1] == 3){
                        points += 2
                    } 
                }
            }
        }
    }
    return points
}

function hatarVidek(){
    let point = 0

    //Sorok
    for(let i = 0; i < board.length; i++){
        if(board[i].every(x => x != 0)){
            point += 6
        }
    }
    //Oszlopok
    for(let i = 0; i < board[0].length; i++){
        let cnt = 0
        for(let j = 0; j < board.length; j++){
            if(board[j][i] != 0){
                cnt++
            }
        }
        if(cnt == 11){
            point += 6
        }
    }

    return point
}

function hillSurrounded(){
    let points = 0
    for(let i = 0; i < hills.length; i++){
        if(board[hills[i].row - 1][hills[i].col - 2] != 0 && board[hills[i].row - 1][hills[i].col] != 0 &&
            board[hills[i].row - 2][hills[i].col - 1] != 0 && board[hills[i].row][hills[i].col - 1] != 0){
                points++
            }
    }
    return points
}

function init(){
    for(let i = 0; i < hills.length; i++){
        board[hills[i].row - 1][hills[i].col-1] = 1
    }
    generateMissions()
    let missionPictures = document.getElementsByTagName("IMG")
    missionPictures[0].classList.add("active")
    missionPictures[1].classList.add("active")
    update()
}

init()
