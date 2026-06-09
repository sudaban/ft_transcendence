import adapter from '@sveltejs/adapter-node';

export default {
  kit: {
    adapter: adapter({
      out: 'build',
      precompress: false
    }),
    env: {
      dir: '..',
      publicPrefix: 'PUBLIC_'
    }
  }
};
