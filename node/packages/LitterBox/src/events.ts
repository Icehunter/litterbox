const FIRE_FORGET = 'FIRE_FORGET';
const FIRE_FORGET_USING_GENERATOR = 'FIRE_FORGET_USING_GENERATOR';
const FLUSH = 'FLUSH';
const RECONNECT = 'RECONNECT';

export const LitterBoxEvents = {
  errors: {
    [FIRE_FORGET]: `${FIRE_FORGET}_ERROR`,
    [FIRE_FORGET_USING_GENERATOR]: `${FIRE_FORGET_USING_GENERATOR}_ERROR`
  }
};

export const TenancyEvents = {
  errors: {
    [FLUSH]: `${FLUSH}_ERROR`,
    [RECONNECT]: `${RECONNECT}_ERROR`
  }
};
