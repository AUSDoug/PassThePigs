// Offscreen 3D pig renderer. MAUI calls window.renderRoll(pose1, dot1, pose2, dot2)
// via EvaluateJavaScriptAsync; it renders and stashes the PNG's base64 in
// window.__b64. MAUI then pulls it back in small slices with window.__b64Slice
// (large return values don't survive the Android WebView JS bridge intact), and
// shows the decoded image in a MAUI Image. The WebView itself is parked far
// offscreen because it doesn't composite reliably on some Android GPU setups.
//
// pig-model.js (POSES + buildPose) is the model exported from Claude Design.

import * as THREE from 'three';
import { buildPose } from './pig-model.js';

const W = 900, H = 810;   // ~matches the on-screen stage box, so the render fills it

const canvas = document.getElementById('c');
canvas.width = W;
canvas.height = H;

const renderer = new THREE.WebGLRenderer({
  canvas, antialias: true, alpha: true, preserveDrawingBuffer: true,
});
renderer.setPixelRatio(1);
renderer.setSize(W, H, false);
renderer.setClearColor(0x000000, 0);
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFShadowMap;

const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(42, W / H, 0.001, 50);

// Studio lighting (from the design's three-d-stage).
scene.add(new THREE.HemisphereLight(0xffffff, 0xd8d2c4, 1.0));
const key = new THREE.DirectionalLight(0xffffff, 2.2);
key.position.set(4, 7, 5);
key.castShadow = true;
key.shadow.mapSize.set(1024, 1024);
key.shadow.bias = -0.0002;
key.shadow.camera.left = -0.12; key.shadow.camera.right = 0.12;
key.shadow.camera.top = 0.12; key.shadow.camera.bottom = -0.12;
key.shadow.camera.near = 0.01; key.shadow.camera.far = 20;
scene.add(key);
const fill = new THREE.DirectionalLight(0xfff4e6, 0.5);
fill.position.set(-5, 3, -4);
scene.add(fill);

const ground = new THREE.Mesh(
  new THREE.PlaneGeometry(2, 2),
  new THREE.ShadowMaterial({ opacity: 0.16 })
);
ground.rotation.x = -Math.PI / 2;
ground.receiveShadow = true;
scene.add(ground);

// Fixed 3/4 view direction; distance is fitted to the pigs each render.
const VIEW_DIR = new THREE.Vector3(0.5, 0.42, 1.4).normalize();
const FILL = 0.9;          // how far the pigs reach toward the tighter frame edge

const _v = new THREE.Vector3();

// World-space vertices of both pigs, so the fit hugs the real silhouette
// (an axis-aligned box would leave slack from its empty corners).
function pigPoints() {
  const pts = [];
  for (const s of anchors) {
    if (!s.pose) continue;
    s.pose.traverse((o) => {
      if (!o.isMesh || !o.geometry?.attributes?.position) return;
      const pos = o.geometry.attributes.position;
      for (let i = 0; i < pos.count; i++) {
        pts.push(_v.fromBufferAttribute(pos, i).applyMatrix4(o.matrixWorld).clone());
      }
    });
  }
  return pts;
}

function frameCamera() {
  scene.updateMatrixWorld(true);
  const pts = pigPoints();
  if (!pts.length) return;

  const box = new THREE.Box3().setFromPoints(pts);
  const c = box.getCenter(new THREE.Vector3());
  // Aim a little below centre: a pig's visual mass sits above its box centre
  // (round body on top, thin legs below), so this reads as vertically centred.
  c.y -= (box.max.y - box.min.y) * 0.06;
  const r = box.getBoundingSphere(new THREE.Sphere()).radius;

  let dist = r * 3;
  camera.near = 0.001;
  camera.far = r * 24;

  for (let iter = 0; iter < 4; iter++) {
    camera.position.copy(c).addScaledVector(VIEW_DIR, dist);
    camera.lookAt(c);
    camera.updateProjectionMatrix();

    let ext = 0;
    for (const p of pts) {
      _v.copy(p).project(camera);
      ext = Math.max(ext, Math.abs(_v.x), Math.abs(_v.y));
    }
    dist *= ext / FILL;
  }

  camera.near = Math.max(dist - r * 2, 0.001);
  camera.far = dist + r * 4;
  camera.updateProjectionMatrix();
}

// The two pigs land near each other, one slightly ahead of the other, so the
// pair reads as a compact cluster rather than a wide row.
const SLOTS = [
  [-0.019, 0, 0.011],
  [0.019, 0, -0.011],
];
const anchors = SLOTS.map(([x, y, z]) => {
  const a = new THREE.Group();
  a.position.set(x, y, z);
  scene.add(a);
  return { anchor: a, pose: null };
});

// buildPig() makes fresh geometry every call, so a discarded pose leaks its
// WebGL buffers until the renderer process is killed (fine on a software GL,
// fatal on a phone GPU after a dozen-odd rolls). Free them here. The materials
// in pig-model.js are shared module singletons - leave those alone.
function disposePose(root) {
  root.traverse((o) => { if (o.isMesh && o.geometry) o.geometry.dispose(); });
}

function setPose(slot, poseId, dot) {
  if (slot.pose) {
    slot.anchor.remove(slot.pose);
    disposePose(slot.pose);
  }
  slot.pose = buildPose(poseId, { dot });
  slot.pose.rotation.y = (Math.random() - 0.5) * 0.8;  // matching poses aren't clones
  slot.pose.traverse((o) => { if (o.isMesh) { o.castShadow = true; o.receiveShadow = true; } });
  slot.anchor.add(slot.pose);
}

// Renders the two pigs and stashes the PNG base64. Returns its length so MAUI
// knows how many slices to pull. (Small integer -> survives the JS bridge.)
window.renderRoll = function renderRoll(pose1, dot1, pose2, dot2) {
  setPose(anchors[0], pose1, !!dot1);
  setPose(anchors[1], pose2, !!dot2);
  frameCamera();
  renderer.render(scene, camera);
  window.__b64 = canvas.toDataURL('image/png').split(',')[1];
  return window.__b64.length;
};

// One slice of the stashed base64 (base64 alphabet has no chars the bridge escapes).
window.__b64Slice = function (start, len) {
  return (window.__b64 || '').substr(start, len);
};

// Live GPU-resource counts; should stay flat across rolls now that poses are
// disposed. Handy from EvaluateJavaScriptAsync if the leak ever regresses.
window.__pigMem = function () {
  const m = renderer.info.memory;
  return `geometries=${m.geometries} textures=${m.textures}`;
};

// flush anything MAUI queued before the module loaded
if (Array.isArray(window.__pigQueue) && window.__pigQueue.length) {
  const last = window.__pigQueue[window.__pigQueue.length - 1];
  window.renderRoll(...last);
}
window.__pigReady = true;
