import { baseSlice } from "./baseSlice";
import { productSlice } from "./productSlice";

export const reducers = {
  base: baseSlice.reducer,
  product: productSlice.reducer,
};
