namespace Service.Converter;

public interface IConverter<TEntity, TDTO>
{
    TEntity ToEntity(TDTO dto);
    TDTO FromEntity(TEntity entity);
}