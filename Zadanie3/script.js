var three = THREE;
var scene = new three.Scene();
var camera = new three.PerspectiveCamera(25, window.innerWidth / window.innerHeight, 0.1, 1000);
var renderer = new three.WebGLRenderer();

renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setClearColor(0x000, 0);
document.body.appendChild(renderer.domElement);

var geometry = new three.BoxGeometry(1, 1, 1);
var colorValue = document.getElementById('colorpicker').value

const light = new THREE.DirectionalLight(0xffffff, 1)
light.position.set(-1, 2, 10)
scene.add(light)

var material = new three.MeshPhongMaterial({
    color: colorValue
});

var cube = new three.Mesh(geometry, material);

cube.rotation.x = Math.PI / 4;
cube.rotation.y = Math.PI / 4;

scene.add(cube);

camera.position.z = 5;

var isDragging = false;
var previousMousePosition = {
    x: 0,
    y: 0
};
$(renderer.domElement).on('mousedown', function (e) {
    isDragging = true;
})
    .on('mousemove', function (e) {
        var deltaMove = {
            x: e.offsetX - previousMousePosition.x,
            y: e.offsetY - previousMousePosition.y
        };

        if (isDragging) {

            var deltaRotationQuaternion = new three.Quaternion()
                .setFromEuler(new three.Euler(
                    toRadians(deltaMove.y * 1),
                    toRadians(deltaMove.x * 1),
                    0,
                    'XYZ'
                ));

            cube.quaternion.multiplyQuaternions(deltaRotationQuaternion, cube.quaternion);
        }

        previousMousePosition = {
            x: e.offsetX,
            y: e.offsetY
        };
    });


$(document).on('mouseup', function (e) {
    isDragging = false;
});

window.requestAnimFrame = (function () {
    return window.requestAnimationFrame ||
        window.webkitRequestAnimationFrame ||
        window.mozRequestAnimationFrame ||
        function (callback) {
            window.setTimeout(callback, 1000 / 60);
        };
})();

var lastFrameTime = new Date().getTime() / 1000;
var totalGameTime = 0;
function update(dt, t) {

    setTimeout(function () {
        var currTime = new Date().getTime() / 1000;
        var dt = currTime - (lastFrameTime || currTime);
        totalGameTime += dt;
        update(dt, totalGameTime);
        lastFrameTime = currTime;
    }, 0);
}

function render() {
    renderer.render(scene, camera);
    requestAnimFrame(render);
}

render();
update(0, totalGameTime);

function updateColor() {
    zpos = cube.rotation.z
    xpos = cube.rotation.x
    ypos = cube.rotation.y
    colorValue = document.getElementById('colorpicker').value

    if (document.getElementById('changeBg').checked) {
        renderer.setClearColor(colorValue, 0);
    }

    material = new three.MeshPhongMaterial({
        color: colorValue
    });
    
    scene.remove(cube)
    cube = new three.Mesh(geometry, material);
    scene.add(cube)
    cube.rotation.x = xpos
    cube.rotation.y = ypos
    cube.rotation.z = zpos
}


function toRadians(angle) {
    return angle * (Math.PI / 180);
}

function toDegrees(angle) {
    return angle * (180 / Math.PI);
}

