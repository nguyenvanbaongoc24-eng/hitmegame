-- DRAFT seed: fake prices pending product approval. Dép is free/default.
insert into public.characters(key,display_name,price_gold) values
('nam_lang','Nam Làng',0),('nu_nang_dong','Nữ Năng Động',200),('quay_boy','Quậy Boy',200),('co_ba','Cô Ba',250),('anh_teen','Anh Teen',250),('cho_vang','Chó Vàng',300),('meo_mun','Mèo Mun',300)
on conflict(key) do update set display_name=excluded.display_name,price_gold=excluded.price_gold;
insert into public.weapons(key,display_name,price_gold) values ('dep','Dép',0) on conflict(key) do update set display_name=excluded.display_name,price_gold=excluded.price_gold;
