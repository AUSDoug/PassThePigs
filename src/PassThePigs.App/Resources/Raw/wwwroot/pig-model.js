import * as THREE from 'three';

const M = {
  skin: new THREE.MeshStandardMaterial({ name: 'pig_skin', color: 0xf0a2ae, roughness: 0.52, metalness: 0.05 }),
  snout: new THREE.MeshStandardMaterial({ name: 'pig_snout', color: 0xe0808f, roughness: 0.45, metalness: 0.05 }),
  ink: new THREE.MeshStandardMaterial({ name: 'pig_ink', color: 0x2b1a1f, roughness: 0.35, metalness: 0.1 }),
  hoof: new THREE.MeshStandardMaterial({ name: 'pig_hoof', color: 0xcf7f8f, roughness: 0.5, metalness: 0.06 }),
};

const mesh = (name, geo, mat) => {
  const m = new THREE.Mesh(geo, mat);
  m.name = name;
  m.castShadow = true;
  m.receiveShadow = true;
  return m;
};

// Toy-scale pig: ~36mm long, modelled in metres, y-up, nose toward +Z.
function buildPig({ dot = true, hips = {} } = {}) {
  const pig = new THREE.Group();
  pig.name = 'pig';

  // barrel: shorter than before so the head reads as its own lobe
  const body = mesh('body', new THREE.SphereGeometry(1, 64, 48), M.skin);
  body.scale.set(0.0112, 0.0118, 0.0142);
  body.position.z = -0.0026;
  pig.add(body);

  // head: a narrower, lower lobe set forward of the barrel — this is the
  // jowl the pig leans on, so it must sit proud of the body silhouette
  const head = mesh('head', new THREE.SphereGeometry(1, 56, 40), M.skin);
  head.scale.set(0.0084, 0.0088, 0.0088);
  head.position.set(0, -0.0014, 0.0122);
  pig.add(head);

  // cheeks/jowls flaring back from the head into the shoulders
  for (const sx of [-1, 1]) {
    const jowl = mesh(`jowl_${sx < 0 ? 'r' : 'l'}`, new THREE.SphereGeometry(1, 40, 28), M.skin);
    jowl.scale.set(0.0046, 0.0052, 0.0058);
    jowl.position.set(sx * 0.0048, -0.0026, 0.0108);
    pig.add(jowl);
  }

  // muzzle taper + snout disc
  const muzzle = mesh('muzzle', new THREE.CylinderGeometry(0.0048, 0.0062, 0.0056, 40), M.skin);
  muzzle.rotation.x = Math.PI / 2;
  muzzle.position.set(0, -0.0022, 0.018);
  pig.add(muzzle);

  const snout = mesh('snout', new THREE.CylinderGeometry(0.0049, 0.0049, 0.0016, 40), M.snout);
  snout.rotation.x = Math.PI / 2;
  snout.position.set(0, -0.0022, 0.0214);
  pig.add(snout);

  for (const sx of [-1, 1]) {
    const nose = mesh(`nostril_${sx < 0 ? 'r' : 'l'}`, new THREE.CylinderGeometry(0.001, 0.001, 0.0012, 20), M.ink);
    nose.rotation.x = Math.PI / 2;
    nose.position.set(sx * 0.0019, -0.0022, 0.0219);
    pig.add(nose);
  }

  // ears: flattened cones laid back along the skull
  for (const sx of [-1, 1]) {
    const ear = mesh(`ear_${sx < 0 ? 'r' : 'l'}`, new THREE.ConeGeometry(0.0036, 0.006, 28), M.skin);
    ear.scale.set(1, 1, 0.34);
    ear.position.set(sx * 0.0062, 0.0048, 0.0104);
    ear.rotation.set(0.62, 0, sx * 0.95);
    pig.add(ear);
  }

  // eyes
  for (const sx of [-1, 1]) {
    const eye = mesh(`eye_${sx < 0 ? 'r' : 'l'}`, new THREE.SphereGeometry(0.0013, 24, 16), M.ink);
    eye.position.set(sx * 0.0054, 0.0022, 0.0176);
    pig.add(eye);
  }

  // four trotters
  const legs = [
    ['fr', -0.0058, 0.0068], ['fl', 0.0058, 0.0068],
    ['br', -0.0058, -0.0102], ['bl', 0.0058, -0.0102],
  ];
  for (const [tag, x, z] of legs) {
    // hip pivot lets a leg swing out without detaching from the body
    const hip = new THREE.Group();
    hip.name = `hip_${tag}`;
    hip.position.set(x, -0.0062, z);
    pig.add(hip);

    const leg = mesh(`leg_${tag}`, new THREE.CylinderGeometry(0.0029, 0.0033, 0.0104, 28), M.skin);
    leg.position.y = -0.0058;
    hip.add(leg);
    const hoof = mesh(`hoof_${tag}`, new THREE.CylinderGeometry(0.0035, 0.0032, 0.002, 28), M.hoof);
    hoof.position.y = -0.0119;
    hip.add(hoof);

    if (hips[tag]) hip.rotation.set(...hips[tag]);
  }

  // curly tail: a short root leaving the rump, then a curl that sits clear
  // of the body (rump surface at this height is z ≈ -0.0155)
  const tailRoot = mesh('tail_root', new THREE.CylinderGeometry(0.0013, 0.0016, 0.0034, 20), M.skin);
  tailRoot.rotation.set(Math.PI / 2 - 0.42, 0, 0);
  tailRoot.position.set(0, 0.0059, -0.0166);
  pig.add(tailRoot);

  const tail = mesh('tail', new THREE.TorusGeometry(0.0027, 0.001, 18, 44, Math.PI * 1.62), M.skin);
  tail.position.set(0, 0.0082, -0.0192);
  tail.rotation.set(0, Math.PI / 2, -0.62);
  pig.add(tail);

  if (dot) {
    // sits proud of the flank (body half-width is 0.0112) so it always reads
    const d = mesh('dot', new THREE.SphereGeometry(0.0026, 32, 20), M.ink);
    d.scale.set(0.2, 1, 1);
    d.position.set(0.0112, 0.0012, -0.0026);
    pig.add(d);
  }

  return pig;
}

// Landing poses: rotation applied to a pivot, then re-grounded so the
// object rests on y = 0 exactly as it would on a table.
export const POSES = [
  { id: 'trotter', label: 'All four trotters', rot: [0, 0, 0], dot: true },
  { id: 'side-up', label: 'Side — dot up', rot: [0, 0, Math.PI / 2], dot: true },
  { id: 'side-down', label: 'Side — dot down', rot: [0, 0, -Math.PI / 2], dot: true },
  { id: 'razorback', label: 'Razorback', rot: [0, 0, Math.PI], dot: true },
  { id: 'snouter', label: 'Snouter', rot: [0.9, 0, 0], dot: true, hips: { fr: [-0.45, 0, 0], fl: [-0.45, 0, 0] } },
  { id: 'jowler', label: 'Leaning jowler', rot: [0.9, 0.2, 1.08], dot: true, hips: { fr: [-0.48, 0, -1.32] } },
];

export function buildPose(poseId, { dot = true } = {}) {
  const pose = POSES.find((p) => p.id === poseId) || POSES[0];
  const root = new THREE.Group();
  root.name = `pig_${pose.id}`;
  const pig = buildPig({ dot, hips: pose.hips || {} });
  pig.rotation.set(...pose.rot);
  root.add(pig);

  root.updateMatrixWorld(true);
  const box = new THREE.Box3().setFromObject(root);
  const c = box.getCenter(new THREE.Vector3());
  pig.position.x -= c.x;
  pig.position.z -= c.z;
  pig.position.y -= box.min.y;
  return root;
}
