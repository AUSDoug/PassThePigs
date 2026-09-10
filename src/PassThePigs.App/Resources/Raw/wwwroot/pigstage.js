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

const W = 900, H = 760;

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

// Camera: fixed frame around the two-slot volume.
{
  const center = new THREE.Vector3(0, 0.012, 0);
  const radius = 0.033;
  const dist = (radius / Math.tan((camera.fov * Math.PI) / 360)) * 1.12;
  const dir = new THREE.Vector3(0.7, 0.5, 1.4).normalize();
  camera.position.copy(center).addScaledVector(dir, dist);
  camera.lookAt(center);
  camera.updateProjectionMatrix();
}

const GAP = 0.028;
const anchors = [-1, 1].map((sx) => {
  const a = new THREE.Group();
  a.position.x = sx * GAP;
  scene.add(a);
  return { anchor: a, pose: null };
});

function setPose(slot, poseId, dot) {
  if (slot.pose) slot.anchor.remove(slot.pose);
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
  renderer.render(scene, camera);
  window.__b64 = canvas.toDataURL('image/png').split(',')[1];
  return window.__b64.length;
};

// One slice of the stashed base64 (base64 alphabet has no chars the bridge escapes).
window.__b64Slice = function (start, len) {
  return (window.__b64 || '').substr(start, len);
};

// flush anything MAUI queued before the module loaded
if (Array.isArray(window.__pigQueue) && window.__pigQueue.length) {
  const last = window.__pigQueue[window.__pigQueue.length - 1];
  window.renderRoll(...last);
}
window.__pigReady = true;
