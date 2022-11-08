convertRGB();
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

// color conversion
// schludnie choc nasrane
function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}
const rgbToHex = (r, g, b) => '#' + [r, g, b].map(x => {
    const hex = x.toString(16)
    return hex.length === 1 ? '0' + hex : hex
}).join('')
function rgb2hsv(r, g, b) {
    let v = Math.max(r, g, b), c = v - Math.min(r, g, b);
    let h = c && ((v == r) ? (g - b) / c : ((v == g) ? 2 + (b - r) / c : 4 + (r - g) / c));
    return [60 * (h < 0 ? h + 6 : h), v && c / v, v];
}
function hsv2rgb(h, s, v) {
    let f = (n, k = (n + h / 60) % 6) => v - v * s * Math.max(Math.min(k, 4 - k, 1), 0);
    return [f(5), f(3), f(1)];
}
function RgbToCmyk(R, G, B) {
    if ((R == 0) && (G == 0) && (B == 0)) {
        return [0, 0, 0, 1];
    } else {
        var calcR = 1 - (R / 255),
            calcG = 1 - (G / 255),
            calcB = 1 - (B / 255);
        var K = Math.min(calcR, Math.min(calcG, calcB)),
            C = (calcR - K) / (1 - K),
            M = (calcG - K) / (1 - K),
            Y = (calcB - K) / (1 - K);
        return [C, M, Y, K];
    }
}
var cmyk2rgb = function (c, m, y, k, normalized) {
    c = (c / 100);
    m = (m / 100);
    y = (y / 100);
    k = (k / 100);

    c = c * (1 - k) + k;
    m = m * (1 - k) + k;
    y = y * (1 - k) + k;

    var r = 1 - c;
    var g = 1 - m;
    var b = 1 - y;

    if (!normalized) {
        r = Math.round(255 * r);
        g = Math.round(255 * g);
        b = Math.round(255 * b);
    }

    return {
        r: r,
        g: g,
        b: b
    }
}

function updateRGB(r, g, b) {
    document.getElementById('colorpicker').value = rgbToHex(r, g, b)
}
function updateCMYK(c, m, y, k) {
    document.getElementById('c').value = c
    document.getElementById('m').value = m
    document.getElementById('y').value = y
    document.getElementById('k').value = k

}
function updateHSV(h, s, v) {
    document.getElementById('h').value = h
    document.getElementById('s').value = s
    document.getElementById('v').value = v / 255
}


function convertRGB() {
    colorValue = hexToRgb(document.getElementById('colorpicker').value)
    hsvValue = rgb2hsv(colorValue.r, colorValue.g, colorValue.b)
    updateHSV(hsvValue[0], hsvValue[1], hsvValue[2])
    cmykValue = RgbToCmyk(colorValue.r, colorValue.g, colorValue.b)
    updateCMYK(cmykValue[0], cmykValue[1], cmykValue[2], cmykValue[3])
}
function convertHSV() {
    h = document.getElementById('h').value
    s = document.getElementById('s').value
    v = document.getElementById('v').value * 255
    rgbValue = hsv2rgb(h, s, v)
    console.log(rgbValue)
    updateRGB(Math.round(rgbValue[0]), Math.round(rgbValue[1]), Math.round(rgbValue[2]))
    cmykValue = RgbToCmyk(rgbValue[0], rgbValue[1], rgbValue[2])
    updateCMYK(cmykValue[0], cmykValue[1], cmykValue[2], cmykValue[3])
}
function convertCMYK() {
    c = document.getElementById('c').value
    m = document.getElementById('m').value
    y = document.getElementById('y').value
    k = document.getElementById('k').value
    rgbValue = cmyk2rgb(c * 100, m * 100, y * 100, k * 100, 0)
    console.log(rgbValue)
    updateRGB(rgbValue.r, rgbValue.g, rgbValue.b)
    hsvValue = rgb2hsv(rgbValue.r, rgbValue.g, rgbValue.b)
    updateHSV(hsvValue[0], hsvValue[1], hsvValue[2])
}